using System;
using CargoWise.EntityFramework;
using Enterprise.DataConverters.CustomsFiles;
using Enterprise.DataConverters.Testing.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework.TestHelper;

namespace Enterprise.DataConverters.Testing.DataWriters
{
	internal abstract class PartWriterTestBase : DataWriterTestCase
	{
		public override void TestAllFieldsInRecordAreImportedProperly()
		{
			var part = (PartWriter)GetNewDataWriter();
			FillInRecordWithUniqueAndCompleteDetails(part);
			AdditionalCountrySpecificSetup(part);

			part.SaveRecordToEnterprise(false, Logger);

			var filter = new ZQuery(OrgSupplierPartSchema.OP_PartNum, part.PartNumber);
			var newRecord = (OrgSupplierPart)Factory.LoadTop1(GetExpectedBusinessObjectType(), filter);
			AssertNotNull("Part should exist", newRecord);

			AssertEquals("PartNumber", part.PartNumber, newRecord.OP_PartNum);
			AssertEquals("Description", part.Description, newRecord.OP_Desc);
			AssertEquals("DefaultStockUnit", part.DefaultStockUnit, newRecord.OP_StockKeepingUnit);
			AssertEquals("Weight", part.Weight, newRecord.OP_Weight);
			AssertEquals("WeightUQ", part.WeightUQ, newRecord.OP_WeightUQ);
			AssertEquals("Volume", part.Volume, newRecord.OP_Cubic);
			AssertEquals("VolumeUQ", part.VolumeUQ, newRecord.OP_CubicUQ);

			Assert("SupplierCode", newRecord.RelatedOrganisations.FindByOrganisationPKAndRelationship(Supplier.PK, OrgPartRelation.RelationshipTypes.Supplier) != null);
			Assert("ImporterCode", newRecord.RelatedOrganisations.FindByOrganisationPKAndRelationship(Importer.PK, OrgPartRelation.RelationshipTypes.Owner) != null);

			AssertCountrySpecificData(part, newRecord);
		}

		protected override void FillInRecordWithUniqueAndCompleteDetails(DataWriter writer)
		{
			var part = writer as PartWriter;
			part.PartNumber = "UNIQUEPART";
			part.Description = "Hopefully a Unique Part Code";
			part.LookupCode = "UNIQUELOOK";
			part.SupplierCode = Supplier.OH_Code;
			part.ImporterCode = Importer.OH_Code;
			part.DefaultStockUnit = "UNT";
			part.Weight = 11m;
			part.WeightUQ = Core.Constants.Weight.Kilograms;
			part.Volume = 0.22m;
			part.VolumeUQ = Core.Constants.Volume.CubicMetres;
		}

		protected override void FillInRecordWithInvalidDetails(DataWriter writer)
		{
			var partWriter = (PartWriter)writer;
			partWriter.PartNumber = "";
		}

		protected Type GetExpectedBusinessObjectType() =>
			TestedTypeHelper.GetTestedType(GetType());

		protected abstract void AssertCountrySpecificData(PartWriter part, OrgSupplierPart bizO);
		protected abstract void AdditionalCountrySpecificSetup(PartWriter part);

		OrgHeader Supplier
		{
			get
			{
				if (fSupplier == null)
				{
					fSupplier = OrgHeader.New(Factory);
					fSupplier.OH_Code = "SUPPLIER";
				}
				return fSupplier;
			}
		}
		OrgHeader fSupplier;

		OrgHeader Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = OrgHeader.New(Factory);
					fImporter.OH_Code = "IMPORTER";
				}
				return fImporter;
			}
		}
		OrgHeader fImporter;
	}
}
