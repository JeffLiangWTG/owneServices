using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AddInfoJobDeclaration))]
	sealed class AddInfoJobDeclarationBOTest : AddInfoBOTest
	{
		public void TestValidationTypeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export Validation", typeof(ExportAddInfoJobDeclarationValidation), declaration.AddInfoValidation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import Validation", typeof(ImportAddInfoJobDeclarationValidation), declaration.AddInfoValidation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
			AssertEquals("Misc Validation", typeof(AddInfoJobDeclarationValidation), declaration.AddInfoValidation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertEquals("LVS Validation", typeof(ImportAddInfoJobDeclarationValidation), declaration.AddInfoValidation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			AssertEquals("LVX Validation", typeof(ImportAddInfoJobDeclarationValidation), declaration.AddInfoValidation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			AssertEquals("B2 Validation", typeof(B2CommonAddInfoJobDeclarationValidation), declaration.AddInfoValidation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			AssertEquals("IM2 Validation", typeof(B2CommonAddInfoJobDeclarationValidation), declaration.AddInfoValidation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			AssertEquals("B3X Validation", typeof(B2CommonAddInfoJobDeclarationValidation), declaration.AddInfoValidation.GetType());
		}

		public void TestCA_CSAEntryHasBeenAddedIntoColumnsForFastSearch()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CA_CSAEntry = true;
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(GenAddOnColumn));
			query.AddToFilter(new ZQuery(GenAddOnColumnSchema.XA_ParentID, declaration.PK));
			query.AddToFilter(new ZQuery(GenAddOnColumnSchema.XA_Name, JobDeclaration.Schema.CA_CSAEntry));
			var column = Factory.LoadTop1<GenAddOnColumn>(query);
			AssertNotNull(column);
		}

		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoJobDeclarationLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(ExportAddInfoJobDeclarationValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			return new AddInfoJobDeclaration(declaration.JE_AddInfoInfo);
		}
	}
}
