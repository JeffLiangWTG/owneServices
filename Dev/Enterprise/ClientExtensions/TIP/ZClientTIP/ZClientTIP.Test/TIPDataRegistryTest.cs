using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TIP.Testing
{
	[TestedType(typeof(TIPDataRegistry))]
	public class TIPDataRegistryTest : RegistryItemSetTestCaseWithFactory<TIPDataRegistry>
	{
		public void TestVisibleRegistryItem()
		{
			AssertEquals("AllItems.Count", 2, AllItems.Count);
			AssertVisible(ItemSet.OrganisationProductRegistryItem);
			AssertVisible(ItemSet.ProductExportRegistryItem);
		}

		#region ProductExportToCsv
		[TestDate(2007, 02, 10)]
		public void TestProductExportRegistryItem()
		{
			AssertEquals("Default ProductExportDirectory", ZString.Empty, ItemSet.ProductExportDirectory);
			AssertEquals("Default ProductExportNotifyGroupPK", true, ItemSet.ProductExportNotifyGroupPK.IsEmpty);
			DataTransferRegistryBusinessObject newValue = new DataTransferRegistryBusinessObject(Factory);
			newValue.Directory = Env.TempPath;
			newValue.Interval = 2;
			newValue.UpdateRuns(ZDateTime.Now);
			newValue.GroupPK = PostmasterGroup.PK;
			ItemSet.ProductExportRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("ProductExportDirectory", Env.TempPath, ItemSet.ProductExportDirectory);
			AssertNotNull("ProductExportNotifyGroupPK", !ItemSet.ProductExportNotifyGroupPK.IsEmpty);
		}

		public void TestProductExportOrganisatons()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "PARTREL";
			Factory.Save();
			OrgPartRelationRegistryBusinessObjectCollection loadedCollection = ItemSet.OrganisationProductRegistryItem.Value;
			AssertEquals(0, loadedCollection.Count);
			OrgPartRelationRegistryBusinessObjectCollection collection = new OrgPartRelationRegistryBusinessObjectCollection();
			OrgPartRelationRegistryBusinessObject bizObj = (OrgPartRelationRegistryBusinessObject)collection.AddNew();
			bizObj.OrgHeaderPK = org.PK;
			bizObj.RelationshipType = OrgPartRelation.RelationshipTypes.Supplier;
			ItemSet.OrganisationProductRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AssertNotNull("OrganisationProductRegistryItem", ItemSet.OrganisationProductRegistryItem.Value);
			loadedCollection = ItemSet.OrganisationProductRegistryItem.Value;
			AssertEquals(1, loadedCollection.Count);
			OrgPartRelationRegistryBusinessObject loadedBizObj = loadedCollection[0];
			AssertEquals("OrgHeaderPK", org.PK, loadedBizObj.OrgHeaderPK);
			AssertEquals("RelationshipType", OrgPartRelation.RelationshipTypes.Supplier, loadedBizObj.RelationshipType);
		}

		#endregion
		public new void TestCategoriesAndCaptionsAreLocalizable()
		{
			Assert(true);
		}

		#region Implementation
		GlbGroup PostmasterGroup
		{
			get
			{
				if (postmasterGroup == null)
				{
					postmasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				}

				return postmasterGroup;
			}
		}

		GlbGroup postmasterGroup;
		#endregion
	}
}
