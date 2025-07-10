using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class VoyageManifestCARSTBusinessObjectLoaderOrCreator : CARSTBusinessObjectLoaderOrCreator
	{
		protected internal override bool IsInterestedInCARST(CMRCARSTMessage message)
		{
			return message.IsSea && message.HouseBillNumber.IsEmpty
				&& (StatusAdviceRelatedToCTO(message) || SeaCTORecordExists(message));
		}

		protected internal override BusinessObject[] LoadOrCreateRecordForMessageCore(CMRCARSTMessage message)
		{
			var oceanBillNumber = message.OceanBillNumber;
			BusinessObject record = !oceanBillNumber.IsEmpty ? LoadOceanBill(message, oceanBillNumber) : LoadCargoLine(message);
			return new BusinessObject[] { record }; // Consider if you need to match more than one.
		}

		BusinessObject LoadOceanBill(CMRCARSTMessage message, ZString oceanBillNumber)
		{
			BusinessObject result = null;
			var autoSendUnderbondOnCARST = (bool)Env.Registry.RawRegistry.AUCAutoSendUnderbondOnCARST.Value;
			var vessel = RefVessel.LookupVesselByLloyds(message.LloydsNumber, message.Factory);
			if (vessel != null)
			{
				var voyageFilter = new ZQuery(CusSeaManTranHeadSchema.BT_VesselName, vessel.RV_Code);
				voyageFilter.AddToFilter(CusSeaManTranHeadSchema.BT_VoyageNum, message.VoyageNumber);

				foreach (var tranHead in message.Factory.Load<CusSeaManTranHead>(voyageFilter))
				{
					foreach (CusSeaManOBLHeader oceanBill in tranHead.OceanBills)
					{
						if (autoSendUnderbondOnCARST)
						{
							foreach (CusSeaManOBLDetail detail in oceanBill.Details)
							{
								CMRAutoUnderbondSender.CheckUnderbondsAndSend(detail.AllUnderbonds.Cast<CusUnderbond>());
							}
						}
						if (oceanBill.BO_OceanBill == oceanBillNumber)
						{
							result = oceanBill;
							break;
						}
					}
					if (result != null)
					{
						break;
					}
				}
			}
			return result;
		}

		BusinessObject LoadCargoLine(CMRCARSTMessage message)
		{
			BusinessObject result = null;

			var lloydsNumber = message.LloydsNumber;
			var voyageNumber = message.VoyageNumber;
			var containerNumber = message.ContainerNumber;

			if (!lloydsNumber.IsEmpty && !voyageNumber.IsEmpty && !containerNumber.IsEmpty)
			{
				var cargoLineQuery = GetCargoLineQuery(lloydsNumber, voyageNumber, containerNumber, message.CARSTSendersReference);
				result = message.Factory.LoadTop1<CusSeaManOBLHeaderCargoLine>(cargoLineQuery);
			}

			return result;
		}

		bool SeaCTORecordExists(CMRCARSTMessage message)
		{
			var hasRecord = HasOceanBill(message);
			if (!hasRecord)
			{
				var lloydsNumber = message.LloydsNumber;
				var voyageNumber = message.VoyageNumber;
				var containerNumber = message.ContainerNumber;

				if (!lloydsNumber.IsEmpty && !voyageNumber.IsEmpty && !containerNumber.IsEmpty)
				{
					hasRecord = HasCargo(message, lloydsNumber, voyageNumber, containerNumber) ||
								HasCargoLine(message, lloydsNumber, voyageNumber, containerNumber);
				}
			}

			return hasRecord;
		}

		ZBool HasOceanBill(CMRCARSTMessage message)
		{
			ZBool hasOceanBill = ZBool.False;

			var oceanBillNumber = message.OceanBillNumber;
			if (!oceanBillNumber.IsEmpty)
			{
				ZQuery oceanBillFilter = new ZQuery(CusSeaManOBLHeaderSchema.BO_OceanBill, oceanBillNumber);
				hasOceanBill = message.Factory.Exists(typeof(CusSeaManOBLHeader), oceanBillFilter);
			}

			return hasOceanBill;
		}

		bool HasCargo(CMRCARSTMessage message, ZString lloydsNumber, ZString voyageNumber, ZString containerNumber)
		{
			var manifestSubQuery = new ZDBOnlySubQuery(typeof(CusSeaManTranHead), CusSeaManOBLHeaderSchema.BO_BT);
			manifestSubQuery.AddToFilter(CusSeaManTranHeadSchema.BT_LloydsIMO, lloydsNumber);
			manifestSubQuery.AddToFilter(CusSeaManTranHeadSchema.BT_VoyageNum, voyageNumber);

			var headerSubQuery = new ZDBOnlySubQuery(typeof(CusSeaManOBLHeader), CusSeaManOBLDetailSchema.BD_BO);
			headerSubQuery.AddSubQuery(manifestSubQuery, JoinCondition.And);

			var detailFilter = new ZDBOnlyQuery(typeof(CusSeaManOBLDetail));
			detailFilter.AddToFilter(CusSeaManOBLDetailSchema.BD_ContainerNumber, containerNumber);
			detailFilter.AddSubQuery(headerSubQuery, JoinCondition.And);

			return message.Factory.Exists(typeof(CusSeaManOBLDetail), detailFilter);
		}

		bool HasCargoLine(CMRCARSTMessage message, ZString lloydsNumber, ZString voyageNumber, ZString containerNumber)
		{
			var cargoLineQuery = GetCargoLineQuery(lloydsNumber, voyageNumber, containerNumber, message.CARSTSendersReference);
			return message.Factory.Exists(typeof(CusSeaManOBLHeaderCargoLine), cargoLineQuery);
		}

		ZQuery GetCargoLineQuery(ZString lloydsNumber, ZString voyageNumber, ZString containerNumber, ZString sendersReference)
		{
			var cargoLineQuery = new ZDBOnlyQuery(typeof(CusSeaManOBLHeaderCargoLine));

			var manifestSubQuery = new ZDBOnlySubQuery(typeof(CusSeaManTranHead), CusSeaManArrivalPortSchema.BA_BT);
			manifestSubQuery.AddToFilter(CusSeaManTranHeadSchema.BT_LloydsIMO, lloydsNumber);
			manifestSubQuery.AddToFilter(CusSeaManTranHeadSchema.BT_VoyageNum, voyageNumber);

			var arrivalSubQuery = new ZDBOnlySubQuery(typeof(CusSeaManArrivalPort), CusSeaManOBLHeaderSchema.BO_BA);
			arrivalSubQuery.AddToFilter(CusSeaManArrivalPortSchema.BA_SendersMessageReference, sendersReference);
			arrivalSubQuery.AddSubQuery(manifestSubQuery, JoinCondition.And);
			cargoLineQuery.AddSubQuery(arrivalSubQuery, JoinCondition.And);

			var detailSubQuery = new ZDBOnlySubQuery(typeof(CusSeaManOBLDetail), CusSeaManOBLDetailSchema.BD_BO);
			detailSubQuery.AddToFilter(CusSeaManOBLDetailSchema.BD_ContainerNumber, containerNumber);
			cargoLineQuery.AddSubQuery(detailSubQuery, JoinCondition.And);

			return cargoLineQuery;
		}
	}
}
