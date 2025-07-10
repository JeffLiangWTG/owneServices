using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class APLMELRunDocsTest : ClientSpecificRunDocsTest
	{
		public APLMELRunDocsTest()
			: base()
		{
		}

		#region Overrides

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		public override BusinessContext BusinessContext
		{
			get { return fBusinessContext; }
		}

		public override BusinessObject GetBusinessObject
		{
			get { return BusinessObjectForTest; }
		}

		protected override ZString ClientName
		{
			get { return "APL"; }
		}

		#endregion

		[ExpectNoExceptions()]
		public void TestCustomsWorksheetFromDeclaration()
		{
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				fBusinessContext = BusinessContext.Customs;
				BusinessObjectForTest = Factory.New<BaseJobDeclaration>();
				RunDocument();
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		[ExpectNoExceptions()]
		public void TestCustomsWorksheetFromShipment()
		{
			fBusinessContext = BusinessContext.Shipment;
			BusinessObjectForTest = Factory.New<CommonShipment>();
			RunDocument();
		}

		#region Implementation
		ZQuery fFilterForMenuItem;
		BusinessContext fBusinessContext;
		BusinessObject BusinessObjectForTest;

		protected override void SetUp()
		{
			base.SetUp();
			fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "APLMEL Customs Worksheet");
		}

		#endregion
	}
}
