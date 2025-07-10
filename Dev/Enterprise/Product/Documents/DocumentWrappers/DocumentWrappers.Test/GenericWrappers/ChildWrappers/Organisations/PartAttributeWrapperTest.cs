using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PartAttributeWrapper))]
	sealed class PartAttributeWrapperTest : GenericWrapperTest
	{
		public void TestValues()
		{
			var organisation = Factory.New<OrgHeader>();
			var partAttrib1 = new PartAttributeWrapper(organisation.MiscServ, 1, Factory);
			AssertEquals("Default name", "Part Attrib. 1", partAttrib1.Name);
			AssertEquals("Default type", ZString.Empty, partAttrib1.Type);
			AssertEquals("Is mandatory", ZBool.False, partAttrib1.IsMandatory);

			organisation.MiscServ.OM_IMPartAttrib1Name = "Serial Number";
			organisation.MiscServ.OM_IMPartAttrib1IsMandatory = ZBool.True;
			organisation.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.VIN;

			organisation.MiscServ.OM_IMPartAttrib2Name = "Batch Number";
			organisation.MiscServ.OM_IMPartAttrib2IsMandatory = ZBool.True;
			organisation.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.BatchNumber;
			organisation.MiscServ.OM_IMUseExpiryDate = true;
			organisation.MiscServ.OM_IMUsePackingDate = true;

			organisation.MiscServ.OM_IMPartAttrib3Name = "Lot Number";
			organisation.MiscServ.OM_IMPartAttrib3IsMandatory = ZBool.False;
			organisation.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.NonMandatory;

			partAttrib1 = new PartAttributeWrapper(organisation.MiscServ, 1, Factory);
			AssertEquals("Name", "Serial Number", partAttrib1.Name);
			AssertEquals("Type", PartAttributeTypeList.Codes.VIN, partAttrib1.Type);
			AssertEquals("Is mandatory", ZBool.True, partAttrib1.IsMandatory);
			AssertEquals("Has exiry date", ZBool.True, partAttrib1.IsExpiryDateUsedByOrganisation);
			AssertEquals("IsPartAttributeUsedByOrganisation", true, partAttrib1.IsPartAttributeUsedByOrganisation);
			AssertEquals("IsSerialNumberUsedByOrganisation", false, partAttrib1.IsSerialNumberUsedByOrganisation);

			var partAttrib2 = new PartAttributeWrapper(organisation.MiscServ, 2, Factory);
			AssertEquals("Name", "Batch Number", partAttrib2.Name);
			AssertEquals("Type", PartAttributeTypeList.Codes.BatchNumber, partAttrib2.Type);
			AssertEquals("Is mandatory", ZBool.True, partAttrib2.IsMandatory);

			var partAttrib3 = new PartAttributeWrapper(organisation.MiscServ, 3, Factory);
			AssertEquals("Name", "Lot Number", partAttrib3.Name);
			AssertEquals("Type", PartAttributeTypeList.Codes.NonMandatory, partAttrib3.Type);
			AssertEquals("Is mandatory", ZBool.False, partAttrib3.IsMandatory);

			organisation.MiscServ.OM_IMUseSerialNumber = true;
			AssertEquals("IsSerialNumberUsedByOrganisation", true, partAttrib1.IsSerialNumberUsedByOrganisation);
		}

		public override void TestWrapperMappingsEmpty()
		{
			PartAttributeWrapper emptyWrapper = new PartAttributeWrapper(null, 0, Factory);
			AssertEquals("Name", ZString.Empty, emptyWrapper.Name);
			AssertEquals("Description", ZString.Empty, emptyWrapper.Description);
			AssertEquals("Type", ZString.Empty, emptyWrapper.Type);
			AssertEquals("IsMandatory", false, emptyWrapper.IsMandatory);
			AssertEquals("IsExpiryDateUsedByOrganisation", false, emptyWrapper.IsExpiryDateUsedByOrganisation);
			AssertEquals("IsPackingDateUsedByOrganisation", false, emptyWrapper.IsPackingDateUsedByOrganisation);
			AssertEquals("IsPartAttributeUsedByOrganisation", false, emptyWrapper.IsPartAttributeUsedByOrganisation);
			AssertEquals("IsSerialNumberUsedByOrganisation", false, emptyWrapper.IsSerialNumberUsedByOrganisation);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			return new PartAttributeWrapper(organisation.MiscServ, 1, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
PartAttribute                                    (Default Field: Name)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Description                             String
IsExpiryDateUsedByOrganisation          Bool
IsMandatory                             Bool
IsPackingDateUsedByOrganisation         Bool
IsPartAttributeUsedByOrganisation       Bool
IsSerialNumberUsedByOrganisation        Bool
Name                                    String
Type                                    String";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			return new PartAttributeWrapper(organisation.MiscServ, 1, Factory);
		}
	}
}
