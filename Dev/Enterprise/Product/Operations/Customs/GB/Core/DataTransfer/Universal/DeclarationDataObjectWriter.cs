using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class DeclarationDataObjectWriter : EU.DataTransfer.Universal.DeclarationDataObjectWriter
	{
		public DeclarationDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateCountrySpecificData(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			base.PopulateCountrySpecificData(declarationData, declarationBO);
			if (declarationBO is JobDeclaration dec)
			{
				var credential = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(dec.CompanyPK.ToGuid(), System.Guid.Empty, System.Guid.Empty).FindByBadgeCode(dec.JE_CustomsProfile);
				if (credential != null)
				{
					var addinfos = declarationData.AddInfoCollection ?? new List<AddInfo>();
					addinfos.AddRange(new[]
					{
						AddInfo.New(Constants.AddInfo.Keys.GatewayKey, credential.Username)
						, AddInfo.New(Constants.AddInfo.Keys.GatewayValue, credential.Password)
						, AddInfo.New(Constants.AddInfo.Keys.GatewayOutputDevice, credential.Printer)
					});
					declarationData.SetAddInfoCollection(() => addinfos);
				}
			}
		}

		protected override void PopulateLocationAtClearanceForWriter(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			var declaration = declarationBO as JobDeclaration;
			if (declaration != null)
			{
				var location = declaration.ApplicationExtender.DataTransferHelper.GetLocationAtClearanceInfoForWritingUXML(declaration);
				if (!location.IsEmpty)
				{
					declarationData.LocationAtClearance = PopulateValue(declarationData.LocationAtClearance, keepExistingData, () => GetLocationAtClearance(location, declarationBO.Lookups.LocationOfGoodsCollection));
				}
			}
		}

		protected override CodeDescriptionPair35Char GetLocationAtClearance(ZString locationOfGoods, IBusinessObjectCollection locationOfGoodsCollection)
		{
			var result = new CodeDescriptionPair35Char()
			{
				Code = locationOfGoods.Left(35),
			};
			return result;
		}

		protected new UniversalDataObjectWriterHelper helper => (UniversalDataObjectWriterHelper)base.helper;

		protected override EU.DataTransfer.Universal.UniversalDataObjectWriterHelper GetUniversalDataObjectWriterHelper(BusinessObjectFactory factory, ZString countryCode)
		{
			return new UniversalDataObjectWriterHelper(factory, countryCode);
		}

		protected override UniversalShipment.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriter()
		{
			return new CustomsEntryHeaderDataObjectWriter(writeManager, helper);
		}
	}
}
