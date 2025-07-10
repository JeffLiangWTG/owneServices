using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class BufferManagementSystemDrawerTest : BMSTestCaseWithFactory
	{
		public void TestExcludeComponentsFromOtherSystemInSchematicPreview()
		{
			var primarySystem = GetPrimarySystemForSchematicTest();
			var secondarySystem = GetSecondarySystemForSchematicTest();

			var donePrimaryBucket = primarySystem.Components.First(c => c.FC_Name == "5 Bucket");
			var secondaryEntryBucket = secondarySystem.Components.First(c => c.FC_Name == "Un Bucket");

			var newLink = donePrimaryBucket.FromMeToOthersLinks.AddNew();
			newLink.FL_FC_ComponentTo = secondaryEntryBucket.PK;

			var drawer = new BufferManagementSystemDrawer();

			drawer.GetSchematicImage(primarySystem, includeNonPrimaryPath: true);

			AssertEquals(4, drawer.DrawnLinks.Count);
			AssertContainsExactElementsInAnyOrder("Only links from the primary system should appear",
				new[] { linkPrimary1, linkPrimary2, linkPrimary3, linkPrimary4 }, drawer.DrawnLinks);
		}

		BMSystem GetPrimarySystemForSchematicTest()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "System 1";

			var component1 = system.Components.AddNew();
			component1.FC_Type = BMComponentTypeList.Codes.Bucket;
			component1.FC_Name = "1 Bucket";
			component1.FC_DisplaySequence = 1;

			var component2 = system.Components.AddNew();
			component2.FC_Type = BMComponentTypeList.Codes.Buffer;
			component2.FC_Name = "2 Buffer";
			component2.FC_BufferTimespanInMinutes = 180;
			component2.FC_DisplaySequence = 2;

			var component3 = system.Components.AddNew();
			component3.FC_Type = BMComponentTypeList.Codes.Bucket;
			component3.FC_Name = "3 Bucket";
			component3.FC_DisplaySequence = 3;

			var component4 = system.Components.AddNew();
			component4.FC_Type = BMComponentTypeList.Codes.Bucket;
			component4.FC_Name = "4 Bucket";
			component4.FC_DisplaySequence = 4;

			var component5 = system.Components.AddNew();
			component5.FC_Type = BMComponentTypeList.Codes.Bucket;
			component5.FC_Name = "5 Bucket";
			component5.FC_DisplaySequence = 5;

			linkPrimary1 = component1.FromMeToOthersLinks.AddNew();
			linkPrimary1.FL_FC_ComponentTo = component2.PK;

			linkPrimary2 = component2.FromMeToOthersLinks.AddNew();
			linkPrimary2.FL_FC_ComponentTo = component3.PK;

			linkPrimary3 = component3.FromMeToOthersLinks.AddNew();
			linkPrimary3.FL_FC_ComponentTo = component4.PK;

			linkPrimary4 = component4.FromMeToOthersLinks.AddNew();
			linkPrimary4.FL_FC_ComponentTo = component5.PK;

			return system;
		}

		BMComponentLink linkPrimary1;
		BMComponentLink linkPrimary2;
		BMComponentLink linkPrimary3;
		BMComponentLink linkPrimary4;

		BMSystem GetSecondarySystemForSchematicTest()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "System 2";

			var component1 = system.Components.AddNew();
			component1.FC_Type = BMComponentTypeList.Codes.Bucket;
			component1.FC_Name = "Un Bucket";
			component1.FC_DisplaySequence = 1;

			var component2 = system.Components.AddNew();
			component2.FC_Type = BMComponentTypeList.Codes.Buffer;
			component2.FC_Name = "Deux Buffer";
			component2.FC_BufferTimespanInMinutes = 180;
			component2.FC_DisplaySequence = 2;

			var component3 = system.Components.AddNew();
			component3.FC_Type = BMComponentTypeList.Codes.Bucket;
			component3.FC_Name = "Trois Bucket";
			component3.FC_DisplaySequence = 3;

			var link1 = component1.FromMeToOthersLinks.AddNew();
			link1.FL_FC_ComponentTo = component2.PK;

			var link2 = component2.FromMeToOthersLinks.AddNew();
			link2.FL_FC_ComponentTo = component3.PK;

			return system;
		}

		/*
		BMSystem GetSystemForSchematicTest()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "Test System";

			var component1 = system.Components.AddNew();
			component1.FC_Type = BMComponentTypeList.Codes.Bucket;
			component1.FC_Name = "1. Future";
			component1.FC_DisplaySequence = 1;

			var component2 = system.Components.AddNew();
			component2.FC_Type = BMComponentTypeList.Codes.Bucket;
			component2.FC_Name = "2. Scheduling";
			component2.FC_DisplaySequence = 2;
			var link = component1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = component2.PK;

			var component3 = system.Components.AddNew();
			component3.FC_Type = BMComponentTypeList.Codes.Buffer;
			component3.FC_Name = "3. Work Buffer";
			component3.FC_DisplaySequence = 3;
			component3.FC_BufferTimespanInMinutes = 360;
			var link2 = component2.FromMeToOthersLinks.AddNew();
			link2.FL_FC_ComponentTo = component3.PK;

			var component4 = system.Components.AddNew();
			component4.FC_Type = BMComponentTypeList.Codes.Bucket;
			component4.FC_Name = "4. Post work";
			component4.FC_DisplaySequence = 4;
			var link3 = component3.FromMeToOthersLinks.AddNew();
			link3.FL_FC_ComponentTo = component4.PK;

			var component5 = system.Components.AddNew();
			component5.FC_Type = BMComponentTypeList.Codes.Constraint;
			component5.FC_Name = "5. Resource Constraint";
			component5.FC_DisplaySequence = 5;
			var link4 = component4.FromMeToOthersLinks.AddNew();
			link4.FL_FC_ComponentTo = component5.PK;

			var component6 = system.Components.AddNew();
			component6.FC_Type = BMComponentTypeList.Codes.Decouple;
			component6.FC_Name = "6. Time Decouple";
			component6.FC_DisplaySequence = 6;
			var link5 = component5.FromMeToOthersLinks.AddNew();
			link5.FL_FC_ComponentTo = component6.PK;
			var link4b = component4.FromMeToOthersLinks.AddNew();
			link4b.FL_FC_ComponentTo = component6.PK;

			var component7 = system.Components.AddNew();
			component7.FC_Type = BMComponentTypeList.Codes.Bucket;
			component7.FC_Name = "7. Done";
			component7.FC_DisplaySequence = 7;
			var link6 = component6.FromMeToOthersLinks.AddNew();
			link6.FL_FC_ComponentTo = component7.PK;
			var link6b = component6.FromMeToOthersLinks.AddNew();
			link6b.FL_FC_ComponentTo = component2.PK;

			var link1 = component1.FromMeToOthersLinks.AddNew();
			link1.FL_FC_ComponentTo = component2.PK;

			var subComponent1 = component3.ChildComponents.AddNew();
			subComponent1.FC_Name = "1. Sub 1";
			subComponent1.FC_DisplaySequence = 1;
			subComponent1.FC_Type = BMComponentTypeList.Codes.Buffer;
			subComponent1.FC_BufferTimespanInMinutes = 100;
			subComponent1.FC_OffsetInMinutes = 30;

			var subComponent2 = component3.ChildComponents.AddNew();
			subComponent2.FC_Name = "2. Sub 2";
			subComponent2.FC_DisplaySequence = 2;
			subComponent2.FC_Type = BMComponentTypeList.Codes.Buffer;
			subComponent2.FC_BufferTimespanInMinutes = 30;
			subComponent2.FC_OffsetInMinutes = 50;

			var subComponent3 = component3.ChildComponents.AddNew();
			subComponent3.FC_Name = "3. Sub 2";
			subComponent3.FC_DisplaySequence = 3;
			subComponent3.FC_Type = BMComponentTypeList.Codes.Constraint;
			subComponent3.FC_OffsetInMinutes = 70;

			return system;
		}

		[RequiresSoftware(RequiredSoftware.Windows8)]
		public void TestGetSchematicImage()
		{
			var system = GetSystemForSchematicTest();
			var drawer = new BufferManagementSystemDrawer();
			var generatedImage = (Bitmap)drawer.GetSchematicImage(system, true);
			using (var expectedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.MasterFiles.GUI.BufferManagement.Testing.SampleSchematicOutput.png"))
			{
				var expectedImage = (Bitmap)Bitmap.FromStream(expectedStream);
				AssertImagePixelsEqual(expectedImage, generatedImage);
			}
		}

		
		[RequiresSoftware(RequiredSoftware.Windows8)]
		public void TestGetSchematicImagePrimaryOnly()
		{
			var system = GetSystemForSchematicTest();
			var drawer = new BufferManagementSystemDrawer();
			var generatedImage = (Bitmap)drawer.GetSchematicImage(system, false);
			using (var expectedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.MasterFiles.GUI.BufferManagement.Testing.SampleSchematicOutputPrimaryOnly.png"))
			{
				var expectedImage = (Bitmap)Bitmap.FromStream(expectedStream);
				AssertImagePixelsEqual(expectedImage, generatedImage);
			}
		}

		
		[RequiresSoftware(RequiredSoftware.Windows8)]
		public void TestGetSchematicImage_ParallelFlows()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "Test System2";

			var component1 = system.Components.AddNew();
			component1.FC_Type = BMComponentTypeList.Codes.Bucket;
			component1.FC_Name = "1. Future";
			component1.FC_DisplaySequence = 1;

			var component2 = system.Components.AddNew();
			component2.FC_Type = BMComponentTypeList.Codes.Bucket;
			component2.FC_Name = "2. Scheduling";
			component2.FC_DisplaySequence = 2;
			var link = component1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = component2.PK;

			var component3 = system.Components.AddNew();
			component3.FC_Type = BMComponentTypeList.Codes.Buffer;
			component3.FC_Name = "3. Work Buffer A";
			component3.FC_DisplaySequence = 3;
			component3.FC_BufferTimespanInMinutes = 360;
			var link2 = component2.FromMeToOthersLinks.AddNew();
			link2.FL_FC_ComponentTo = component3.PK;

			var component4 = system.Components.AddNew();
			component4.FC_Type = BMComponentTypeList.Codes.Bucket;
			component4.FC_Name = "4. Post work A";
			component4.FC_DisplaySequence = 4;
			var link3 = component3.FromMeToOthersLinks.AddNew();
			link3.FL_FC_ComponentTo = component4.PK;

			var component5 = system.Components.AddNew();
			component5.FC_Type = BMComponentTypeList.Codes.Buffer;
			component5.FC_Name = "5. Work Buffer B";
			component5.FC_DisplaySequence = 5;
			component5.FC_BufferTimespanInMinutes = 150;
			var link4 = component2.FromMeToOthersLinks.AddNew();
			link4.FL_FC_ComponentTo = component5.PK;

			var component6 = system.Components.AddNew();
			component6.FC_Type = BMComponentTypeList.Codes.Bucket;
			component6.FC_Name = "6. Post work B";
			component6.FC_DisplaySequence = 6;
			var link5 = component5.FromMeToOthersLinks.AddNew();
			link5.FL_FC_ComponentTo = component6.PK;

			var component7 = system.Components.AddNew();
			component7.FC_Type = BMComponentTypeList.Codes.Bucket;
			component7.FC_Name = "7. Done";
			component7.FC_DisplaySequence = 7;
			var link6 = component6.FromMeToOthersLinks.AddNew();
			link6.FL_FC_ComponentTo = component7.PK;

			var link7 = component4.FromMeToOthersLinks.AddNew();
			link7.FL_FC_ComponentTo = component7.PK;

			var drawer = new BufferManagementSystemDrawer();
			var generatedImage = (Bitmap)drawer.GetSchematicImage(system, true);
			using (var expectedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.MasterFiles.GUI.BufferManagement.Testing.SampleSchematicOutput_Parallel.png"))
			{
				var expectedImage = (Bitmap)Bitmap.FromStream(expectedStream);
				AssertImagePixelsEqual(expectedImage, generatedImage);
			}
		}

		[RequiresSoftware(RequiredSoftware.Windows8)]
		public void TestGetLegendImage()
		{
			var drawer = new BufferManagementSystemDrawer();
			var generatedImage = (Bitmap)drawer.GetLegendImage();
			using (var expectedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.MasterFiles.GUI.BufferManagement.Testing.SampleLegendOutput.png"))
			{
				var expectedImage = (Bitmap)Bitmap.FromStream(expectedStream);
				AssertImagePixelsEqual(expectedImage, generatedImage);
			}
		}
		 * */
	}
}
