using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class BufferedItemBufferPenetrationViewModelCollectionTest : NetworkTestCase
	{
		public void TestBuild_ForBuffer_ShouldAddRelatedItems()
		{
			var collection = new BufferedItemBufferPenetrationViewModelCollection((IBuffer)projectBuffer).Cast<BufferedItemBufferPenetrationViewModel>().ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { "shape1", "shape2", "shape3", "shape4" }, collection.Select(i => i.BufferedItemName));

			collection = new BufferedItemBufferPenetrationViewModelCollection((IBuffer)feedingBuffer).Cast<BufferedItemBufferPenetrationViewModel>().ToArray();
			AssertEquals(1, collection.Length);

			AssertCollectionContains(collection, i => i.BufferedItemName == "shape4");
		}

		public void TestBuild_ForShape_ShouldAddRelatedBuffers()
		{
			var collection = new BufferedItemBufferPenetrationViewModelCollection(shape1).Cast<BufferedItemBufferPenetrationViewModel>().ToArray();
			AssertEquals(1, collection.Length);

			AssertCollectionContains(collection, i => i.BufferName == "Project Buffer");

			collection = new BufferedItemBufferPenetrationViewModelCollection(shape4).Cast<BufferedItemBufferPenetrationViewModel>().ToArray();
			AssertEquals(2, collection.Length);

			AssertCollectionContains(collection, i => i.BufferName == "Project Buffer");
			AssertCollectionContains(collection, i => i.BufferName == "shape4 Feeding Buffer");
		}

		[TestDate(2014, 8, 22)]
		public void TestSortByPenetration_ShouldSortNumerically()
		{
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1); // To force update on workflow status

			Factory.Save();

			var collection = new BufferedItemBufferPenetrationViewModelCollection((IBuffer)projectBuffer);
			var property = typeof(BufferedItemBufferPenetrationViewModel).GetProperty("PenetrationPercent");

			CombineAssertions("Buffer penetrations", () =>
			{
				AssertEquals("Penetration for shape1", "150 %", collection.Cast<BufferedItemBufferPenetrationViewModel>().Single(p => p.BufferedItemName == "shape1").PenetrationPercent);
				AssertEquals("Penetration for shape2", "90 %", collection.Cast<BufferedItemBufferPenetrationViewModel>().Single(p => p.BufferedItemName == "shape2").PenetrationPercent);
				AssertEquals("Penetration for shape3", "30 %", collection.Cast<BufferedItemBufferPenetrationViewModel>().Single(p => p.BufferedItemName == "shape3").PenetrationPercent);
				AssertEquals("Penetration for shape4 (should use overflow from feeding buffer penetration)", "110 %", collection.Cast<BufferedItemBufferPenetrationViewModel>().Single(p => p.BufferedItemName == "shape4").PenetrationPercent);
			});

			collection.Sort(property.Name, ListSortDirection.Ascending);

			AssertEquals(4, collection.Count);

			CombineAssertions("Sorting by penetration percent Ascending", () =>
			{
				AssertEquals("Penetration at position 0: " + collection[0].PenetrationPercent, "shape3", collection[0].BufferedItemName);
				AssertEquals("Penetration at position 1: " + collection[1].PenetrationPercent, "shape2", collection[1].BufferedItemName);
				AssertEquals("Penetration at position 2: " + collection[2].PenetrationPercent, "shape4", collection[2].BufferedItemName);
				AssertEquals("Penetration at position 3: " + collection[3].PenetrationPercent, "shape1", collection[3].BufferedItemName);
			});

			collection.Sort(property.Name, ListSortDirection.Descending);

			CombineAssertions("Sorting by penetration percent Descending", () =>
			{
				AssertEquals("Penetration at position 0: " + collection[0].PenetrationPercent, "shape1", collection[0].BufferedItemName);
				AssertEquals("Penetration at position 1: " + collection[1].PenetrationPercent, "shape4", collection[1].BufferedItemName);
				AssertEquals("Penetration at position 2: " + collection[2].PenetrationPercent, "shape2", collection[2].BufferedItemName);
				AssertEquals("Penetration at position 3: " + collection[3].PenetrationPercent, "shape3", collection[3].BufferedItemName);
			});
		}

		ProcessJobHeader jobHeader;
		ProcessHeader workflow1, workflow2, workflow3, workflow4;
		BMNCNShape shape1, shape2, shape3, shape4;
		BMNCNBufferShape projectBuffer, feedingBuffer;
		IJobNetwork network;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.CreateSystem(Factory, "ORG");
			jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.Now.AddDays(-2);

			workflow1 = CreateWorkflow(jobHeader, "workflow1");
			workflow2 = CreateWorkflow(jobHeader, "workflow2");
			workflow3 = CreateWorkflow(jobHeader, "workflow3");
			workflow4 = CreateWorkflow(jobHeader, "workflow4");

			var diagram = CreateDiagram(jobHeader, name: "Diagram");
			shape1 = CreateShape(workflow1, diagram, "shape1");
			shape2 = CreateShape(workflow2, diagram, "shape2");
			shape3 = CreateShape(workflow3, diagram, "shape3");
			shape4 = CreateShape(workflow4, diagram, "shape4");

			// 1 -> 2 -> 3
			//      4 -> 3

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			shape4.MakeVisiblePrerequisiteOf(shape3, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram, createNodeViewModels: false);
			network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			Factory.Save();

			projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single(b => b.Type == BufferType.Project);
			feedingBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single(b => b.Type == BufferType.Feeding);
		}
	}

	[TestedType(typeof(BufferedItemBufferPenetrationViewModelCollection))]
	class BufferedItemBufferPenetrationCollectionNonPersistentBizoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BufferedItemBufferPenetrationViewModelCollection>
	{
		public void TestAddNew_ShouldNotBeSupported()
		{
			var collection = GetCollectionToTest();
			AssertExceptionThrown<NotSupportedException>(() => collection.AddNew());
		}

		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, GetCollectionToTest().AllowRemove);
		}

		protected override BufferedItemBufferPenetrationViewModelCollection GetCollectionToTest()
		{
			return new BufferedItemBufferPenetrationViewModelCollection((IBuffer)GetBuffer());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BufferedItemBufferPenetrationViewModel(GetBuffer(), Factory.New<BMNCNShape>());
		}

		BMNCNBufferShape GetBuffer()
		{
			var buffer = Factory.New<BMNCNBufferShape>();
			buffer.ExplicitDurationMinutes = 60 * BMConstants.WorkingHoursPerDay;

			return buffer;
		}
	}
}
