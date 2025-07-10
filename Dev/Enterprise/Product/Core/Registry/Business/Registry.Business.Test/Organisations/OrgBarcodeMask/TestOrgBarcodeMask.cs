using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgBarcodeMask))]
	sealed class TestOrgBarcodeMask : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidation()
		{
			var org = Factory.New<IOrgHeader>();
			org.OH_Code = "ORG";
			org.OH_IsPackDepot = true;

			var address = Factory.New<IOrgAddress>();
			address.OA_Address1 = "ADDRESS";
			address.OA_Code = "CODE";

			address.OA_OH = org.PK;

			Factory.Save();

			OrgBarcodeMask orgBarcodeMask = (OrgBarcodeMask)GetNewBusinessObject();
			orgBarcodeMask.Org = org.PK;
			orgBarcodeMask.Priority = 1;
			orgBarcodeMask.Mask = "(ONE)GROUP";
			CombineAssertions("No errors should be displayed with valid values", delegate
			{
				AssertNoErrors("Org", orgBarcodeMask.OrgInfo);
				AssertNoErrors("Priority", orgBarcodeMask.PriorityInfo);
				AssertNoErrors("Mask", orgBarcodeMask.MaskInfo);
			});

			org.OH_IsPackDepot = false;
			Factory.Save();

			orgBarcodeMask = (OrgBarcodeMask)GetNewBusinessObject();
			orgBarcodeMask.Org = org.PK;
			orgBarcodeMask.Priority = 1;
			orgBarcodeMask.Mask = "(HAS)MULTIPLE(GROUPS)";

			CombineAssertions("Errors should be displayed with with invalid values", delegate
			{
				AssertHasErrors(orgBarcodeMask.OrgInfo);
				AssertHasErrors(orgBarcodeMask.PriorityInfo);
				AssertHasErrors(orgBarcodeMask.MaskInfo);
			});
		}

		OrgBarcodeMaskCollection collection;

		OrgBarcodeMaskCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new OrgBarcodeMaskCollection(Factory);
				}
				return collection;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Collection.AddNew();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new OrgBarcodeMask(null, Factory, new OrgBarcodeMaskCollection());
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new OrgBarcodeMask(null, Factory, new OrgBarcodeMaskCollection());
		}
	}
}
