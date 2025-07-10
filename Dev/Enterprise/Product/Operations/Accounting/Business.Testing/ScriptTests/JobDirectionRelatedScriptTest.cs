using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class JobDirectionRelatedScriptTest : TestCaseWithFactory
	{
		public void TestStaticIsLocal()
		{
			AssertEquals("Local", "EXP", GetJobDirection(LocalPort, ZString.Empty));
			AssertNotEquals("Foreign", "EXP", GetJobDirection(ForeignPort, ZString.Empty));
		}

		public void TestStaticIsImport()
		{
			AssertEquals("Import", true, ImportExportHelper.IsImport(ForeignPort, LocalPort));
			AssertEquals("Import", true, MatchWithADirection(Directions.Import, ForeignPort, LocalPort));

			var homePortReference = new Mock<ILocationReference>();
			homePortReference.Setup(m => m.IsLocalInRelationTo(ForeignPort)).Returns(true);
			AssertEquals("Export", false, ImportExportHelper.IsImport(ForeignPort, LocalPort, homePortReference.Object));
			AssertEquals("Export", false, MatchWithADirection(Directions.Import, ForeignPort, LocalPort, DifferentHomePort));

			AssertEquals("Export", false, ImportExportHelper.IsImport(LocalPort, ForeignPort));
			AssertEquals("Export", false, MatchWithADirection(Directions.Import, LocalPort, ForeignPort));

			AssertEquals("Trans-Shipment", false, ImportExportHelper.IsImport(ForeignPort, ForeignPort));
			AssertEquals("Trans-Shipment", false, MatchWithADirection(Directions.Import, ForeignPort, ForeignPort));

			AssertEquals("Domestic", false, ImportExportHelper.IsImport(LocalPort, LocalPort));
			AssertEquals("Domestic", false, MatchWithADirection(Directions.Import, LocalPort, LocalPort));

			AssertEquals("Import Missing-Origin", !IsDomestic, ImportExportHelper.IsImport("", LocalPort));
			AssertEquals("Import Missing-Origin", !IsDomestic, MatchWithADirection(Directions.Import, "", LocalPort));

			AssertEquals("Export Missing-Origin", false, ImportExportHelper.IsImport("", ForeignPort));
			AssertEquals("Export Missing-Origin", false, MatchWithADirection(Directions.Import, "", ForeignPort));

			AssertEquals("Import Missing-Destination", true, ImportExportHelper.IsImport(ForeignPort, ""));
			AssertEquals("Import Missing-Destination", true, MatchWithADirection(Directions.Import, ForeignPort, ""));

			AssertEquals("Export Missing-Destination", false, ImportExportHelper.IsImport(LocalPort, ""));
			AssertEquals("Export Missing-Destination", false, MatchWithADirection(Directions.Import, LocalPort, ""));

			AssertEquals("Missing-Both", IsImport, ImportExportHelper.IsImport("", ""));
			AssertEquals("Missing-Both", IsImport, MatchWithADirection(Directions.Import, "", ""));
		}

		public void TestStaticIsExport()
		{
			AssertEquals("Import", false, ImportExportHelper.IsExport(ForeignPort, LocalPort));
			AssertEquals("Import", false, MatchWithADirection(Directions.Export, ForeignPort, LocalPort));

			AssertEquals("Export", true, ImportExportHelper.IsExport(LocalPort, ForeignPort));
			AssertEquals("Export", true, MatchWithADirection(Directions.Export, LocalPort, ForeignPort));

			var homePortReference = new Mock<ILocationReference>();
			homePortReference.Setup(m => m.IsLocalInRelationTo(ForeignPort)).Returns(true);
			AssertEquals("Import", false, ImportExportHelper.IsExport(LocalPort, ForeignPort, homePortReference.Object));
			AssertEquals("Import", false, MatchWithADirection(Directions.Export, LocalPort, ForeignPort, DifferentHomePort));

			AssertEquals("Trans-Shipment", false, ImportExportHelper.IsExport(ForeignPort, ForeignPort));
			AssertEquals("Trans-Shipment", false, MatchWithADirection(Directions.Export, ForeignPort, ForeignPort));

			AssertEquals("Trans-Shipment, but orgin considered Local", true, ImportExportHelper.IsExport(ForeignPort, ForeignPort2, homePortReference.Object));
			AssertEquals("Trans-Shipment, but orgin considered Local", true, MatchWithADirection(Directions.Export, ForeignPort, ForeignPort2, DifferentHomePort));

			AssertEquals("Trans-Shipment, but destination considered Local", false, ImportExportHelper.IsExport(ForeignPort2, ForeignPort, homePortReference.Object));
			AssertEquals("Trans-Shipment, but destination considered Local", false, MatchWithADirection(Directions.Export, ForeignPort2, ForeignPort, DifferentHomePort));

			AssertEquals("Domestic", false, ImportExportHelper.IsExport(LocalPort, LocalPort));
			AssertEquals("Domestic", false, MatchWithADirection(Directions.Export, LocalPort, LocalPort));

			AssertEquals("Import Missing-Origin", false, ImportExportHelper.IsExport("", LocalPort));
			AssertEquals("Import Missing-Origin", false, MatchWithADirection(Directions.Export, "", LocalPort));

			AssertEquals("Export Missing-Origin", true, ImportExportHelper.IsExport("", ForeignPort));
			AssertEquals("Export Missing-Origin", true, MatchWithADirection(Directions.Export, "", ForeignPort));

			AssertEquals("Import Missing-Destination", false, ImportExportHelper.IsExport(ForeignPort, ""));
			AssertEquals("Import Missing-Destination", false, MatchWithADirection(Directions.Export, ForeignPort, ""));

			AssertEquals("Import or Trans? Missing-Destination, but orgin considered Local", true, ImportExportHelper.IsExport(ForeignPort, "", homePortReference.Object));
			AssertEquals("Import or Trans? Missing-Destination, but orgin considered Local", true, MatchWithADirection(Directions.Export, ForeignPort, "", DifferentHomePort));

			AssertEquals("Export Missing-Destination", !IsDomestic, ImportExportHelper.IsExport(LocalPort, ""));
			AssertEquals("Export Missing-Destination", !IsDomestic, MatchWithADirection(Directions.Export, LocalPort, ""));

			AssertEquals("Missing-Both", IsExport, ImportExportHelper.IsExport("", ""));
			AssertEquals("Missing-Both", IsExport, MatchWithADirection(Directions.Export, "", ""));
		}

		#region Cross Trade

		public void TestStaticIsCrossTrade()
		{
			AssertEquals("Import", false, ImportExportHelper.IsCrossTrade(ForeignPort, LocalPort));
			AssertEquals("Import", false, MatchWithADirection(Directions.CrossTrade, ForeignPort, LocalPort));

			AssertEquals("Export", false, ImportExportHelper.IsCrossTrade(LocalPort, ForeignPort));
			AssertEquals("Export", false, MatchWithADirection(Directions.CrossTrade, LocalPort, ForeignPort));

			AssertEquals("Domestic", false, ImportExportHelper.IsCrossTrade(ForeignPort, ForeignPort));
			AssertEquals("Domestic", false, MatchWithADirection(Directions.CrossTrade, ForeignPort, ForeignPort));

			AssertEquals("Trans-Shipment", true, ImportExportHelper.IsCrossTrade(ForeignPort, ForeignPort2));
			AssertEquals("Trans-Shipment", true, MatchWithADirection(Directions.CrossTrade, ForeignPort, ForeignPort2));

			AssertEquals("Trans-Shipment", true, ImportExportHelper.IsCrossTrade(ForeignPort2, ForeignPort));
			AssertEquals("Trans-Shipment", true, MatchWithADirection(Directions.CrossTrade, ForeignPort2, ForeignPort));

			var homePortReference = new Mock<ILocationReference>();
			homePortReference.Setup(m => m.IsLocalInRelationTo(ForeignPort)).Returns(true);
			AssertEquals("Not a Trans-Shipment", false, ImportExportHelper.IsCrossTrade(ForeignPort2, ForeignPort, homePortReference.Object));
			AssertEquals("Not a Trans-Shipment", false, MatchWithADirection(Directions.CrossTrade, ForeignPort2, ForeignPort, DifferentHomePort));

			AssertEquals("Domestic", false, ImportExportHelper.IsCrossTrade(LocalPort, LocalPort));
			AssertEquals("Domestic", false, MatchWithADirection(Directions.CrossTrade, LocalPort, LocalPort));

			AssertEquals("Import Missing-Origin", false, ImportExportHelper.IsCrossTrade("", LocalPort));
			AssertEquals("Import Missing-Origin", false, MatchWithADirection(Directions.CrossTrade, "", LocalPort));

			AssertEquals("Export Missing-Origin", false, ImportExportHelper.IsCrossTrade("", ForeignPort));
			AssertEquals("Export Missing-Origin", false, MatchWithADirection(Directions.CrossTrade, "", ForeignPort));

			AssertEquals("Import Missing-Destination", false, ImportExportHelper.IsCrossTrade(ForeignPort, ""));
			AssertEquals("Import Missing-Destination", false, MatchWithADirection(Directions.CrossTrade, ForeignPort, ""));

			AssertEquals("Export Missing-Destination", false, ImportExportHelper.IsCrossTrade(LocalPort, ""));
			AssertEquals("Export Missing-Destination", false, MatchWithADirection(Directions.CrossTrade, LocalPort, ""));

			AssertEquals("Missing-Both", false, ImportExportHelper.IsCrossTrade("", ""));
			AssertEquals("Missing-Both", false, MatchWithADirection(Directions.CrossTrade, "", ""));
		}

		public void TestStaticIsCrossTrade_SameCountry()
		{
			var query = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Germany);
			var country = Factory.LoadTop1<RefCountry>(query);

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new Guid[] { country.PK.ToGuid() });
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("CrossTrade", false, ImportExportHelper.IsCrossTrade("DEHAM", "DEFRA"));
				AssertEquals("CrossTrade", false, MatchWithADirection(Directions.CrossTrade, "DEHAM", "DEFRA"));
			}
		}

		#endregion

		public void TestStaticIsDomestic()
		{
			AssertEquals("Import", false, ImportExportHelper.IsDomestic(ForeignPort, LocalPort));
			AssertEquals("Import", false, MatchWithADirection(Directions.Domestic, ForeignPort, LocalPort));

			AssertEquals("Export", false, ImportExportHelper.IsDomestic(LocalPort, ForeignPort));
			AssertEquals("Export", false, MatchWithADirection(Directions.Domestic, LocalPort, ForeignPort));

			AssertEquals("Trans-Shipment", false, ImportExportHelper.IsDomestic(ForeignPort, ForeignPort2));
			AssertEquals("Trans-Shipment", false, MatchWithADirection(Directions.Domestic, ForeignPort, ForeignPort2));

			AssertEquals("Domestic", true, ImportExportHelper.IsDomestic(ForeignPort, ForeignPort));
			AssertEquals("Domestic", true, MatchWithADirection(Directions.Domestic, ForeignPort, ForeignPort));

			AssertEquals("Domestic", true, ImportExportHelper.IsDomestic(LocalPort, LocalPort));
			AssertEquals("Domestic", true, MatchWithADirection(Directions.Domestic, LocalPort, LocalPort));

			AssertEquals("Import Missing-Origin", false, ImportExportHelper.IsDomestic("", LocalPort));
			AssertEquals("Import Missing-Origin", false, MatchWithADirection(Directions.Domestic, "", LocalPort));

			AssertEquals("Export Missing-Origin", false, ImportExportHelper.IsDomestic("", ForeignPort));
			AssertEquals("Export Missing-Origin", false, MatchWithADirection(Directions.Domestic, "", ForeignPort));

			AssertEquals("Import Missing-Destination", false, ImportExportHelper.IsDomestic(ForeignPort, ""));
			AssertEquals("Import Missing-Destination", false, MatchWithADirection(Directions.Domestic, ForeignPort, ""));

			AssertEquals("Export Missing-Destination", false, ImportExportHelper.IsDomestic(LocalPort, ""));
			AssertEquals("Export Missing-Destination", false, MatchWithADirection(Directions.Domestic, LocalPort, ""));

			AssertEquals("Missing-Both", false, ImportExportHelper.IsDomestic("", ""));
			AssertEquals("Missing-Both", false, MatchWithADirection(Directions.Domestic, "", ""));
		}

		#region TestGetJobDirection_CommunityRegion

		public void TestGetJobDirection_CommunityRegion()
		{
			var query1 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Sweden);
			var query2 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);

			var country1 = Factory.LoadTop1<RefCountry>(query1);
			var country2 = Factory.LoadTop1<RefCountry>(query2);

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { country1.PK.ToGuid(), country2.PK.ToGuid() });

			AssertEquals("Foreign -> Local", Directions.Import, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.Sweden));
			AssertEquals("Foreign -> Local", "IMP", GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.Sweden));

			AssertEquals("Local -> Foreign", Directions.Export, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.NewZealand));
			AssertEquals("Local -> Foreign", "EXP", GetJobDirection(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.NewZealand));

			AssertEquals("Local -> Local", Directions.CrossTrade, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.Sweden, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("Local -> Local", "CRO", GetJobDirection(Core.Constants.CountryCodes.Sweden, Core.Constants.CountryCodes.UnitedStates));

			AssertEquals("Foreign -> Foreign", Directions.CrossTrade, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.NewZealand));
			AssertEquals("Foreign -> Foreign", "CRO", GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.NewZealand));

			AssertEquals("Local -> Home", Directions.Import, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.UnitedStates, LocalPort));
			AssertEquals("Local -> Home", "IMP", GetJobDirection(Core.Constants.CountryCodes.UnitedStates, LocalPort));

			AssertEquals("Home -> Local", Directions.Export, ImportExportHelper.GetJobDirection(LocalPort, Core.Constants.CountryCodes.Sweden));
			AssertEquals("Home -> Local", "EXP", GetJobDirection(LocalPort, Core.Constants.CountryCodes.Sweden));

			AssertEquals("Home -> Home", Directions.Domestic, ImportExportHelper.GetJobDirection(LocalPort, LocalPort));
			AssertEquals("Home -> Home", "DOM", GetJobDirection(LocalPort, LocalPort));
		}

		#endregion

		public void TestGetJobDirection_CommunityRegion_SwitchCompany()
		{
			var query1 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Sweden);

			var country1 = Factory.LoadTop1<RefCountry>(query1);

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new Guid[] { country1.PK.ToGuid() });
			AssertEquals("Foreign -> Local", Directions.Import, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.Sweden));
			AssertEquals("Foreign -> Local", "IMP", GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.Sweden));

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			newCompanyBranch.GB_GC = newCompany.PK;

			Factory.Save();

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Array.Empty<Guid>());
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Foreign -> Foreign", Directions.CrossTrade, ImportExportHelper.GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.Sweden));
				AssertEquals("Foreign -> Foreign", "CRO", GetJobDirection(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.Sweden));
			}
		}

		#region TestJobDirectionForCommunityRegions

		public void TestJobDirectionForCommunityRegions()
		{
			var query1 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.NewZealand);
			var query2 = new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);

			var country1 = Factory.LoadTop1<RefCountry>(query1);
			var country2 = Factory.LoadTop1<RefCountry>(query2);

			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { country1.PK.ToGuid(), country2.PK.ToGuid() });

			var homePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var origin = Core.Constants.CountryCodes.India;
			var destination = Core.Constants.CountryCodes.UnitedStates;

			AssertEquals("origin outside region and destination within region", Directions.Import, ImportExportHelper.GetJobDirection(origin, destination));
			AssertEquals("origin outside region and destination within region", "IMP", GetJobDirection(origin, destination));

			origin = Core.Constants.CountryCodes.UnitedStates;
			destination = Core.Constants.CountryCodes.India;

			AssertEquals("origin within region and destination outside region", Directions.Export, ImportExportHelper.GetJobDirection(origin, destination));
			AssertEquals("origin within region and destination outside region", "EXP", GetJobDirection(origin, destination));

			origin = Core.Constants.CountryCodes.UnitedStates;
			destination = Core.Constants.CountryCodes.NewZealand;

			AssertEquals("origin and destination within same region", Directions.CrossTrade, ImportExportHelper.GetJobDirection(origin, destination));
			AssertEquals("origin and destination within same region", "CRO", GetJobDirection(origin, destination));

			origin = Core.Constants.CountryCodes.India;
			destination = Core.Constants.CountryCodes.Canada;

			AssertEquals("origin and destination outside region", Directions.CrossTrade, ImportExportHelper.GetJobDirection(origin, destination));
			AssertEquals("origin and destination outside region","CRO", GetJobDirection(origin, destination));

			origin = Core.Constants.CountryCodes.UnitedStates;
			destination = homePort.Substring(0, 2);

			AssertEquals("origin within region and destination in logged in country", Directions.Import, ImportExportHelper.GetJobDirection(origin, destination));
			AssertEquals("origin within region and destination in logged in country", "IMP", GetJobDirection(origin, destination));

			origin = homePort.Substring(0, 2);
			destination = Core.Constants.CountryCodes.NewZealand;

			AssertEquals("origin in logged in country and destination within region", Directions.Export, ImportExportHelper.GetJobDirection(origin, destination));
			AssertEquals("origin in logged in country and destination within region", "EXP", GetJobDirection(origin, destination));

			origin = homePort.Substring(0, 2);
			destination = homePort.Substring(0, 2);

			AssertEquals("origin and destination in logged in country", Directions.Domestic, ImportExportHelper.GetJobDirection(origin, destination));
			AssertEquals("origin and destination in logged in country", "DOM", GetJobDirection(origin, destination));
		}

		#endregion

		#region Implementation

		ZString GetJobDirection(ZString origin, ZString destination)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, "SELECT Direction FROM dbo.GetJobDirection('{0}', '{1}', '{2}', '{3}', {4}, {5}, {6}, '{7}')",
										origin,
										destination,
										LocalPort,
										LocalPort,
										Convert.ToInt32(IsDomestic),
										Convert.ToInt32(IsExport),
										Convert.ToInt32(IsImport),
										Env.CurrentCompanyPK);

			var direction = DataUtils.GetListOfValuesFromQuery(TestConnection, sql).FirstOrDefault();
			return direction;
		}

		bool MatchWithADirection(Directions macthWith, ZString origin, ZString destination, ZString? homePortReference = null)
		{
			var result = false;
			var functionSQL = string.Empty;
			switch (macthWith)
			{
				case Directions.Import:
					functionSQL = string.Format(CultureInfo.InvariantCulture, "dbo.IsImport('{0}', '{1}', '{2}', '{3}', {4}, {5}, '{6}')",
										origin,
										destination,
										homePortReference ?? LocalPort,
										LocalPort,
										Convert.ToInt32(IsDomestic),
										Convert.ToInt32(IsImport),
										Env.CurrentCompanyPK);
					break;
				case Directions.Export:
					functionSQL = string.Format(CultureInfo.InvariantCulture, "dbo.IsExport('{0}', '{1}', '{2}', '{3}', {4}, {5}, '{6}')",
										origin,
										destination,
										homePortReference ?? LocalPort,
										LocalPort,
										Convert.ToInt32(IsDomestic),
										Convert.ToInt32(IsExport),
										Env.CurrentCompanyPK);
					break;
				case Directions.Domestic:
					functionSQL = string.Format(CultureInfo.InvariantCulture, "dbo.IsDomestic('{0}', '{1}' ,'{2}', {3}, '{4}')",
										origin,
										destination,
										homePortReference ?? LocalPort,
										Convert.ToInt32(IsDomestic),
										Env.CurrentCompanyPK);
					break;
				case Directions.CrossTrade:
					functionSQL = string.Format(CultureInfo.InvariantCulture, "dbo.IsCrossTrade('{0}', '{1}' ,'{2}', '{3}')",
										origin,
										destination,
										homePortReference ?? LocalPort,
										Env.CurrentCompanyPK);
					break;
				default:
					break;
			}
			var sql = string.Format("SELECT Result FROM {0}", functionSQL);

			result = Convert.ToBoolean(Convert.ToInt32(DataUtils.GetListOfValuesFromQuery(TestConnection, sql).FirstOrDefault()));

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			IsDomestic = GlbDepartment.CurrentDepartment.GE_Domestic;
			IsImport = GlbDepartment.CurrentDepartment.GE_Import;
			IsExport = GlbDepartment.CurrentDepartment.GE_Export;

			LocalPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZQuery notLocal = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, LocalPort.Substring(0, 2));
			ForeignPort = (Factory.LoadTop1<RefUNLOCO>(notLocal)).RL_Code;

			var query = notLocal.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, ForeignPort);
			DifferentHomePort = (Factory.LoadTop1<RefUNLOCO>(query)).RL_Code;

			ZQuery notLocal2 = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, LocalPort.Substring(0, 2));
			notLocal2.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, ForeignPort.Substring(0, 2));
			ForeignPort2 = (Factory.LoadTop1<RefUNLOCO>(notLocal2)).RL_Code;
		}

		ZString LocalPort;
		ZString ForeignPort;
		ZString ForeignPort2;
		ZString DifferentHomePort;
		ZBool IsDomestic;
		ZBool IsImport;
		ZBool IsExport;

		#endregion
	}
}
