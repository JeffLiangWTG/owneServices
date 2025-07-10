using System.Drawing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappersCore.Testing
{
	public static class BrandingTestHelperClass
	{
		public static void SetClientBrandRegistryImage()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("11", "Desc");
			list.AddPair("22", "Desc");
			list.AddPair("33", "Desc");

			ClientTariffAndLevelCollection collection = new ClientTariffAndLevelCollection();
			var element = collection.AddNew();
			element.Code = "11";
			element.Description = (NoResString)"Desc";
			element.BrandName = "blah1";
			element.BrandEmailAddress = "blah1@blah.com";
			element.Image = new Bitmap(1, 1);
			element.CodeList.AddRange(list);

			element = collection.AddNew();
			element.Code = "22";
			element.Description = (NoResString)"Desc";
			element.BrandName = "blah2";
			element.BrandEmailAddress = "blah2@blah.com";
			element.Image = new Bitmap(2, 2);
			element.CodeList.AddRange(list);

			element = collection.AddNew();
			element.Code = "33";
			element.Description = (NoResString)"Desc";
			element.BrandName = "blah3";
			element.BrandEmailAddress = "blah3@generic.com";
			element.Image = new Bitmap(3, 3);
			element.UseGeneric = ZBool.False;
			element.CodeList.AddRange(list);

			DocumentsDataRegistry.Instance.ClientTariffAndLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, collection);
		}

		public static void SetAgentBrandRegistryImage()
		{
			AgentDocumentBrandCollection collection = new AgentDocumentBrandCollection();
			var element1 = collection.AddNew();
			element1.CodeList.AddPair("STD", "Desc");
			element1.Code = "STD";
			element1.Description = (NoResString)"Desc";
			element1.BrandName = "blah1";
			element1.BrandEmailAddress = "blah1@blah.com";
			element1.Image = new Bitmap(5, 5);

			var element2 = collection.AddNew();
			element2.CodeList.AddPair("11", "Desc");
			element2.Code = "11";
			element2.Description = (NoResString)"Desc";
			element2.BrandName = "blah2";
			element2.BrandEmailAddress = "blah2@blah.com";
			element2.Image = new Bitmap(9, 9);

			var element3 = collection.AddNew();
			element3.CodeList.AddPair("55", "Desc");
			element3.Code = "55";
			element3.Description = (NoResString)"Desc";
			element3.BrandName = "blah3";
			element3.BrandEmailAddress = "blah3@blah.com";
			element3.Image = new Bitmap(8, 8);

			var element4 = collection.AddNew();
			element4.CodeList.AddPair("GNR", "Agent branding with generic email");
			element4.Code = "GNR";
			element4.Description = (NoResString)"Agent branding with generic email";
			element4.BrandName = "Agent branding with generic email";
			element4.BrandEmailAddress = "blah3@generic.com";
			element4.UseGeneric = ZBool.False;
			element4.Image = new Bitmap(9, 9);

			DocumentsDataRegistry.Instance.AgentDocumentBrand.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, collection);
		}

		public static void SetHybridBrandRegistryImage(HybridDocumentBrandCollectionRegistryItem registry)
		{
			HybridDocumentBrandCollection collection = new HybridDocumentBrandCollection();
			var element1 = (ClientAndAgentBrandingBusinessObject)collection.AddNew();
			element1.CodeList.AddPair("STD", "Desc");
			element1.Code = "STD";
			element1.Description = (NoResString)"Desc";
			element1.Image = new Bitmap(5, 5);

			var element2 = (ClientAndAgentBrandingBusinessObject)collection.AddNew();
			element2.CodeList.AddPair("11", "Desc");
			element2.Code = "11";
			element2.Description = (NoResString)"Desc";
			element2.Image = new Bitmap(9, 9);

			var element3 = (ClientAndAgentBrandingBusinessObject)collection.AddNew();
			element3.CodeList.AddPair("55", "Desc");
			element3.Code = "55";
			element3.Description = (NoResString)"Desc";
			element3.Image = new Bitmap(8, 8);

			registry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, collection);
		}
	}
}
