using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class AdditionalAddInfoGroupCollectionDataObjectWriterForCargoControlNumber : IAdditionalAddInfoGroupCollectionDataObjectWriter
	{
		public AdditionalAddInfoGroupCollectionDataObjectWriterForCargoControlNumber(CargoControlNumber cargoControlNumber, JobDeclaration declaration)
		{
			this.cargoControlNumber = Argument.NotNull(cargoControlNumber, "CargoControlNumber");
			this.declaration = Argument.NotNull(declaration, "JobDeclaration");
		}
		readonly CargoControlNumber cargoControlNumber;
		readonly JobDeclaration declaration;

		#region IAdditionalAddInfoGroupCollectionDataObjectWriter Members

		public IEnumerable<AddInfoGroup> CreateCollection()
		{
			var linkedBillNumber = ZString.Empty;
			var linkedBillType = ZString.Empty;
			var releaseStatus = declaration.ReleaseStatuses.OfType<ReleaseStatus>().FirstOrDefault(x => x.RL_CargoControlNumber == cargoControlNumber.CY_CargoControlNumber);
			if (releaseStatus != null && releaseStatus.Bills.Count > 0)
			{
				var linkedBill = releaseStatus.Bills.OfType<Bill>().FirstOrDefault(x => x.PK == releaseStatus.RL_Bill);
				if (linkedBill != null)
				{
					linkedBillType = linkedBill.CU_BillType;
					linkedBillNumber = linkedBill.CU_BillNum;
				}
			}

			if (!cargoControlNumber.CY_CargoControlNumber.IsEmpty || !linkedBillNumber.IsEmpty || !linkedBillType.IsEmpty)
			{
				yield return new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = Constants.AddInfoKeys.CargoControlNumber.Type, Description = "Cargo Control Number" },
					AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>(new[]
					{
						CreateAddInfo(Constants.AddInfoKeys.CargoControlNumber.CCNumber, cargoControlNumber.CY_CargoControlNumber),
						CreateAddInfo(Constants.AddInfoKeys.CargoControlNumber.BillType, linkedBillType),
						CreateAddInfo(Constants.AddInfoKeys.CargoControlNumber.BillNumber, linkedBillNumber)
					})
				};
			}
		}

		UniversalDataBuss.DataObjects.Universal.AddInfo CreateAddInfo(ZString key, ZString value)
		{
			return new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = key, Value = value };
		}

		#endregion
	}
}
