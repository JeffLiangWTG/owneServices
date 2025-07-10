using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.ReportTesting.Testing
{
	abstract class Report_GbDeclarationsExport_ParametersFiltersTest : ReportFunctionalFilterTestCase<JobDeclaration>
	{
		protected override JobDeclaration MakeNewBusinessObject()
		{
			DeclarationReportTestHelper.MakeDeclaration(Factory, out var declaration);
			if (declaration.CustomsEntryHeaders.Count > 1)
			{
				for (int i = 1; i < declaration.CustomsEntryHeaders.Count; i++)
				{
					declaration.CustomsEntryHeaders[i].Delete();
				}
			}
			return declaration;
		}

		protected override SqlObjectType SqlObjectType => Enterprise.ReportTesting.SqlObjectType.FunctionTable;

		protected override ZString ObjectName => "Report_GbDeclarationsExport";

		protected override List<string> ParametersValuesListUnadulterated => DeclarationReportTestHelper.GetParametersValuesListExport();
	}

	namespace FilterTests
	{
		// The following classes each vary the report on one parameter at a time. 
		// They will make a pair of decs, vary one field, run the report with a special value in one parameter, 
		// check that one row is returned by the report and the other is excluded. 

		class FilterTransportMode : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_TransportMode; }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return (ZString)Core.Constants.TransportModes.Other; }
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return (ZString)Core.Constants.TransportModes.Air; }
			}

			protected override string AliasedFieldReturnedByReport
			{
				get { return "Box25TransportMode"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 1; } // Transport mode
			}
		}

		class FilterOrigin : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_RL_NKOrigin; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 2; }
			}

			protected override string AliasedFieldReturnedByReport
			{
				get { return "Box15PortOfOrigin"; }
			}
		}

		class FilterDestination : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_RL_NKFinalDestination; }
			}

			protected override string AliasedFieldReturnedByReport
			{
				get { return "Box17PortOfDestination"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 3; }
			}
		}

		class FilterLoading : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_RL_NKPortOfLoading; }
			}

			protected override string AliasedFieldReturnedByReport
			{
				get { return "PortOfLoading"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 4; }
			}
		}

		class FilterArrival : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_RL_NKPortOfArrival; }
			}

			protected override string AliasedFieldReturnedByReport
			{
				get { return "PortofDischarge"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 5; }
			}
		}

		class FilterCreateDateFrom : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_SystemCreateTimeUtc; }
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return (ZString)ZDateTime.BrettsBirthday.AddDays(-1).ToISO8601String(); }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return (ZString)ZDateTime.BrettsBirthday.AddDays(1).ToISO8601String(); }
			}
			protected override string AliasedFieldReturnedByReport
			{
				get { return "CreatedTime"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 6; }
			}
		}

		class FilterCreateDateTo : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_SystemCreateTimeUtc; }
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return (ZString)ZDateTime.Today.AddDays(+1).ToISO8601String(); }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return (ZString)ZDateTime.Today.AddDays(-1).ToISO8601String(); }
			}
			protected override string AliasedFieldReturnedByReport
			{
				get { return "CreatedTime"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 7; }
			}
		}

		class FilterImporter : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			OrgHeader orgGood;
			OrgHeader orgBad;
			protected override void SetUp()
			{
				base.SetUp();
				orgGood = Factory.NewWithValidTestData<OrgHeader>();
				orgBad = Factory.NewWithValidTestData<OrgHeader>();
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return orgBad.PK; }
			}
			protected override IZType ValueToSetOnInclude
			{
				get { return orgGood.PK; }
			}

			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_OH_Importer; }
			}

			protected override string AliasedFieldReturnedByReport
			{
				get { return "Box8ImporterCode"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 8; }
			}

			protected override IZType ValueBackThatShouldNotBePresentInFoundRow
			{
				get { return orgBad.OH_Code; }
			}

			protected override IZType ValueToExpectBackForFoundRow
			{
				get { return orgGood.OH_Code; }
			}
		}

		class FilterExporter : FilterImporter
		{
			protected override string AliasedFieldReturnedByReport
			{
				get { return "Box2SupplierCode"; }
			}

			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_OH_Supplier; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 9; }
			}
		}

		class FilterBranch : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			GlbBranch good;
			GlbBranch bad;
			protected override void SetUp()
			{
				base.SetUp();
				good = GlbCompany.CurrentCompany.Branches.AddNew();
				good.GB_Code = "GGG";
				bad = GlbCompany.CurrentCompany.Branches.AddNew();
				bad.GB_Code = "BAD";
				GlbCompany.CurrentCompany.Factory.Save();
			}
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_GB; }
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return bad.PK; }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return good.PK; }
			}
			protected override string AliasedFieldReturnedByReport
			{
				get { return "BranchCode"; }
			}

			protected override IZType ValueToExpectBackForFoundRow
			{
				get { return good.GB_Code; }
			}

			protected override IZType ValueBackThatShouldNotBePresentInFoundRow
			{
				get { return bad.GB_Code; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 10; }
			}
		}

		class FilterDepartureDateFrom : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_ExportDate; }
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return (ZString)ZDateTime.Today.AddDays(-1).ToISO8601String(); }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return (ZString)ZDateTime.Today.AddDays(+1).ToISO8601String(); }
			}
			protected override string AliasedFieldReturnedByReport
			{
				get { return "DepartureDate"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 11; }
			}
		}

		class FilterDepartureDateTo : FilterDepartureDateFrom
		{
			protected override int ParameterIndexToReplace
			{
				get { return 12; }
			}
			protected override IZType ValueToSetOnExclude
			{
				get { return (ZString)ZDateTime.Today.AddDays(+1).ToISO8601String(); }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return (ZString)ZDateTime.Today.AddDays(-1).ToISO8601String(); }
			}
		}

		class FilterArrivalDateFrom : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return JobDeclarationSchema.JE_DateOfArrival; }
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return (ZString)ZDateTime.Today.AddDays(-1).ToISO8601String(); }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return (ZString)ZDateTime.Today.AddDays(+1).ToISO8601String(); }
			}
			protected override string AliasedFieldReturnedByReport
			{
				get { return "ArrivalDate"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 13; }
			}
		}

		class FilterArrivalDateTo : FilterArrivalDateFrom
		{
			protected override int ParameterIndexToReplace
			{
				get { return 14; }
			}
			protected override IZType ValueToSetOnExclude
			{
				get { return (ZString)ZDateTime.Today.AddDays(+1).ToISO8601String(); }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return (ZString)ZDateTime.Today.AddDays(-1).ToISO8601String(); }
			}
		}

		class FilterRouteOfEntry : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return null; }
			}

			protected override string AliasedFieldReturnedByReport
			{
				get { return "RouteOfEntry"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 15; }
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return new ZString("A"); }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return new ZString("B"); }
			}

			protected override MethodToSetValue SetValueManually
			{
				get
				{
					return delegate(JobDeclaration dec, IZType value, bool isExclude)
					{
						if (!isExclude)
						{
							dec.JE_GBRouteOfEntry = (ZString)value;
						}
					};
				}
			}
		}

		class FilterDeclarationType : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return null; }
			}

			protected override string AliasedFieldReturnedByReport
			{
				get { return "Box1DeclarationType"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 16; }
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return new ZString("ECR"); }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return new ZString("EFD"); }
			}

			protected override MethodToSetValue SetValueManually
			{
				get
				{
					return delegate(JobDeclaration dec, IZType value, bool isExclude)
					{
						dec.JE_DeclarationType = (ZString)value;
					};
				}
			}
		}
		class FilterBadge : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return null; }
			}

			protected override string AliasedFieldReturnedByReport
			{
				get { return "Badge"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 17; }
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return new ZString("ABC"); }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return new ZString("DEF"); }
			}

			protected override MethodToSetValue SetValueManually
			{
				get
				{
					return delegate(JobDeclaration dec, IZType value, bool isExclude)
					{
						dec.JE_CustomsProfile = (ZString)value;
					};
				}
			}
		}
		class FilterCSP : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return null; }
			}

			protected override string AliasedFieldReturnedByReport
			{
				get { return "CSP"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 18; }
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return new ZString("MCP"); }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return new ZString("CCSUK"); }
			}

			protected override MethodToSetValue SetValueManually
			{
				get
				{
					return delegate(JobDeclaration dec, IZType value, bool isExclude)
					{
						dec.ZG_Gateway = (ZString)value;
					};
				}
			}
		}
		class FilterRepresentation : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer
			{
				get { return null; }
			}

			protected override string AliasedFieldReturnedByReport
			{
				get { return "Box14RepresentationType"; }
			}

			protected override int ParameterIndexToReplace
			{
				get { return 19; }
			}

			protected override IZType ValueToSetOnExclude
			{
				get { return new ZString("DIR"); }
			}

			protected override IZType ValueToSetOnInclude
			{
				get { return new ZString("IND"); }
			}

			protected override MethodToSetValue SetValueManually
			{
				get
				{
					return delegate(JobDeclaration dec, IZType value, bool isExclude)
					{
						dec.JE_DeclarantType = (ZString)value;
					};
				}
			}
		}

		class FilterCDSLocation : Report_GbDeclarationsExport_ParametersFiltersTest
		{
			protected override SchemaColumn NameOfBusinessObjectFieldToSetViaIndexer => null;
			protected override string AliasedFieldReturnedByReport => "CdsLocationOfGoods";
			protected override int ParameterIndexToReplace => 22;
			protected override IZType ValueToSetOnExclude => new ZString("XYZABC");
			protected override IZType ValueToSetOnInclude => new ZString("ABCDEF");
			protected override MethodToSetValue SetValueManually => (JobDeclaration dec, IZType value, bool _) => dec.JE_GoodsLocation = (ZString)value;
		}
	}
}
