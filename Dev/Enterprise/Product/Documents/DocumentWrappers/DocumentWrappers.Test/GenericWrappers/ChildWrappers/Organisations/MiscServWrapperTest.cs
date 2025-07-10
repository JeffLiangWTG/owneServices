using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(MiscServWrapper))]
	sealed class MiscServWrapperTest : GenericWrapperTest
	{
		public void TestAttributes()
		{
			OrgMisServ.OM_IMPartAttrib1Name = "PartAttrib1";
			OrgMisServ.OM_IMPartAttrib2Name = "PartAttrib2";
			OrgMisServ.OM_IMPartAttrib3Name = "PartAttrib3";

			AssertEquals("IMPartAttrib1Name", OrgMisServ.OM_IMPartAttrib1Name, MiscServWrapper.PartAttribute1.Name);
			AssertEquals("IMPartAttrib2Name", OrgMisServ.OM_IMPartAttrib2Name, MiscServWrapper.PartAttribute2.Name);
			AssertEquals("IMPartAttrib3Name", OrgMisServ.OM_IMPartAttrib3Name, MiscServWrapper.PartAttribute3.Name);
		}

		public void TestClientDocumentLogo()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var header = factory.New<OrgHeader>();
			header.OH_FullName = "Test Org";
			header.MainAddress.OA_Address1 = "Test Address";
			header.OH_RL_NKClosestPort = "AUSYD";
			OrgMiscServ miscServ = header.MiscServ;

			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var memStream = resourceRetriever.GetBytes("Enterprise.DocumentWrappers.Testing.AWBMasterTitle.png");

			var note = factory.New<StmNote>();
			note.ST_NoteData = memStream;
			note.ST_ParentID = miscServ.PK;
			note.ST_Table = miscServ.TableName;
			note.ST_NoteType = nameof(StmNoteVisibility.DOC);
			note.ST_Description = "ClientDocumentLogo";
			factory.Save();

			AssertNotNull(miscServ.ClientDocumentLogo);
		}

		public void DGContact()
		{
			AssertNotNull(MiscServWrapper.DGContact);
		}

		OrgMiscServ OrgMisServ
		{
			get
			{
				if (orgMisServ == null)
				{
					orgMisServ = Factory.New<OrgHeader>().MiscServ;
				}
				return orgMisServ;
			}
		}
		OrgMiscServ orgMisServ;

		MiscServWrapper MiscServWrapper
		{
			get { return miscServWrapper ?? (miscServWrapper = new MiscServWrapper(OrgMisServ, Factory)); }
		}
		MiscServWrapper miscServWrapper;

		public override void TestWrapperMappingsEmpty()
		{
			MiscServWrapper emptyWrapper = new MiscServWrapper(null, Factory);
			AssertNull("DGContact", emptyWrapper.DGContact);
			AssertNull("PartAttribute1", emptyWrapper.PartAttribute1);
			AssertNull("PartAttribute2", emptyWrapper.PartAttribute2);
			AssertNull("PartAttribute3", emptyWrapper.PartAttribute3);
			AssertNull("ClientDocumentLogo", emptyWrapper.ClientDocumentLogo);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return MiscServWrapper;
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
MiscServ
======================================================================
Name                                    Type
----------------------------------------------------------------------
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return MiscServWrapper;
		}
	}
}
