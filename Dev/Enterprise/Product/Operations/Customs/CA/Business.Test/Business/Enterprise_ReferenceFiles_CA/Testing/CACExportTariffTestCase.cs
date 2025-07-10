using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class CACExportTariffTestCase : TestCaseWithFactory
	{
		public CACExportTariffTestCase()
		{
		}

		public CACExportTariffTestCase(BusinessObjectFactory factory)
		{
			fFactory = factory;
		}

		#region Tariff

		#region Tariff64059000
		public TariffView Tariff64059000
		{
			get
			{
				if (fTariff64059000 == null)
				{
					fTariff64059000 = CreateNewTariffIfNotExists("64059000", "Footwear, nes", UnitOfMeasureListForDLM.Codes.Pair);
				}
				return fTariff64059000;
			}
		}
		TariffView fTariff64059000;
		#endregion

		#region Tariff84289020
		public TariffView Tariff84289020
		{
			get
			{
				if (fTariff84289020 == null)
				{
					fTariff84289020 = CreateNewTariffIfNotExists("84289020", "Machinery of kind used in handling radio-active materials", UnitOfMeasureListForDLM.Codes.NotApplicable);
				}
				return fTariff84289020;
			}
		}
		TariffView fTariff84289020;
		#endregion

		#region Tariff85164000
		public TariffView Tariff85164000
		{
			get
			{
				if (fTariff85164000 == null)
				{
					fTariff85164000 = CreateNewTariffIfNotExists("85164000", "Electric smoothing irons", UnitOfMeasureListForDLM.Codes.Number);
				}
				return fTariff85164000;
			}
		}
		TariffView fTariff85164000;
		#endregion

		#region Tariff87032330
		public TariffView Tariff87032330
		{
			get
			{
				if (fTariff87032330 == null)
				{
					fTariff87032330 = CreateNewTariffIfNotExists("87032330", "Automobiles, used w reciprocating piston engine displacing > 1500 cc to 3000 cc", UnitOfMeasureListForDLM.Codes.Number, Constants.RefCusTariffAttribute.AttributeValue.Y);
				}
				return fTariff87032330;
			}
		}
		TariffView fTariff87032330;
		#endregion

		public TariffView CreateNewTariffIfNotExists(ZString tariff, ZString description, string unit = "", string conveyanceRequired = "")
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariffView = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, tariff, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, description);
			if (!unit.IsNullOrEmpty())
			{
				universalHelper.CreateTariffUOM(tariffView, "CU1", unit);
			}
			if (!conveyanceRequired.IsNullOrEmpty())
			{
				universalHelper.CreateNewOrGetExistingTariffAttribute(Constants.RefCusTariffAttribute.AttributeName.ConveyanceRequired, conveyanceRequired, tariffView);
			}

			return tariffView;
		}

		#endregion

		#region Implementation

		protected override BusinessObjectFactory NewFactory()
		{
			BusinessObjectFactory result = fFactory;
			if (fFactory == null)
			{
				result = base.NewFactory();
			}
			return result;
		}

		readonly BusinessObjectFactory fFactory;

		#endregion
	}
}
