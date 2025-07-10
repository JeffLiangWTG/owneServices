using System.Linq;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(AsyncShapeFloatCalculator))]
	class AsyncShapeFloatCalculatorTest : ValidatedAsyncOperatorTestCase<AsyncShapeFloatCalculator, IJobNetwork, ScheduleNodeSnapshot>
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override IValidatedAsyncOperator GetNewOperator()
		{
			return new AsyncShapeFloatCalculator();
		}

		protected override ScheduleNodeSnapshot GetExpectedInitialSnapshot()
		{
			var network = new SimpleTestNetwork(Factory).Network;
			return new ScheduleNodeSnapshot(network);
		}

		protected override IJobNetwork GetNewInitialReal()
		{
			return new SimpleTestNetwork(Factory).Network;
		}

		protected override IJobNetwork GetRealWithIncompatibleChanges()
		{
			return IncompatibleNetwork;
		}

		#region Transformation Real

		protected override ScheduleNodeSnapshot GetExpectedResultSnapshot()
		{
			var network = new SimpleTestNetwork(Factory);
			var snapshot = new ScheduleNodeSnapshot(network.Network);

			snapshot.ScheduleNetwork.SchedulesByEntity[network.Network.DiagramEntity.PK] = new NetworklessScheduleNode(network.Network.DiagramEntity)
			{
				EarliestStartHours = 0,
				LatestStartHours = 0,
				IsCriticalPath = false,
				EstimatedDurationHoursIncludingChildren = 4,
			};

			snapshot.ScheduleNetwork.SchedulesByEntity[network.Shape1.PK] = new NetworklessScheduleNode(network.Shape1)
			{
				EarliestStartHours = 0,
				LatestStartHours = 0,
				IsCriticalPath = true,
				EstimatedDurationHoursIncludingChildren = 1,
			};

			snapshot.ScheduleNetwork.SchedulesByEntity[network.Shape2_cc.PK] = new NetworklessScheduleNode(network.Shape2_cc)
			{
				EarliestStartHours = 1,
				LatestStartHours = 1,
				IsCriticalPath = true,
				EstimatedDurationHoursIncludingChildren = 2,
			};

			snapshot.ScheduleNetwork.SchedulesByEntity[network.Shape2_noncc.PK] = new NetworklessScheduleNode(network.Shape2_noncc)
			{
				EarliestStartHours = 1,
				LatestStartHours = 2,
				IsCriticalPath = false,
				EstimatedDurationHoursIncludingChildren = 1,
			};

			snapshot.ScheduleNetwork.SchedulesByEntity[network.Shape3.PK] = new NetworklessScheduleNode(network.Shape3)
			{
				EarliestStartHours = 3,
				LatestStartHours = 3,
				IsCriticalPath = true,
				EstimatedDurationHoursIncludingChildren = 1,
			};

			return snapshot;
		}

		protected override IJobNetwork GetExpectedReal()
		{
			var network = new SimpleTestNetwork(Factory);

			network.Network.DiagramEntity.Schedule = new NetworklessScheduleNode(network.Network.DiagramEntity)
			{
				EarliestStartHours = 0,
				LatestStartHours = 0,
				IsCriticalPath = false,
				EstimatedDurationHoursIncludingChildren = 4,
			};

			network.Shape1.AsEntity(network.Network).Schedule = new NetworklessScheduleNode(network.Shape1)
			{
				EarliestStartHours = 0,
				LatestStartHours = 0,
				IsCriticalPath = true,
				EstimatedDurationHoursIncludingChildren = 1,
			};

			network.Shape2_cc.AsEntity(network.Network).Schedule = new NetworklessScheduleNode(network.Shape2_cc)
			{
				EarliestStartHours = 1,
				LatestStartHours = 1,
				IsCriticalPath = true,
				EstimatedDurationHoursIncludingChildren = 2,
			};

			network.Shape2_noncc.AsEntity(network.Network).Schedule = new NetworklessScheduleNode(network.Shape2_noncc)
			{
				EarliestStartHours = 1,
				LatestStartHours = 2,
				IsCriticalPath = false,
				EstimatedDurationHoursIncludingChildren = 1,
			};

			network.Shape3.AsEntity(network.Network).Schedule = new NetworklessScheduleNode(network.Shape3)
			{
				EarliestStartHours = 3,
				LatestStartHours = 3,
				IsCriticalPath = true,
				EstimatedDurationHoursIncludingChildren = 1,
			};

			return network.Network;
		}

		#endregion

		#region Equals overloads

		protected override bool SnapshotEquals(ScheduleNodeSnapshot first, ScheduleNodeSnapshot second)
		{
			if (first.ScheduleNetwork.SchedulesByEntity.Count != second.ScheduleNetwork.SchedulesByEntity.Count)
			{
				return false;
			}
			else
			{
				foreach (var pair in OrderSchedules(first).Zip(OrderSchedules(second), (f1, f2) => new { First = f1, Second = f2 }))
				{
					if (!ScheduleEquals(pair.First, pair.Second))
					{
						return false;
					}
				}
				return true;
			}
		}

		static IOrderedEnumerable<ScheduleNode> OrderSchedules(ScheduleNodeSnapshot snapshot)
		{
			return snapshot.ScheduleNetwork.SchedulesByEntity.Values.OrderBy(v => v.Entity.DisplayName);
		}

		protected override bool RealEquals(IJobNetwork first, IJobNetwork second)
		{
			if (first.Shapes.Count == second.Shapes.Count)
			{
				foreach (var shape in first.Shapes)
				{
					var equivalentShape = second.Shapes.FirstOrDefault(s => s.BNS_Name == shape.BNS_Name);
					if (equivalentShape == null || !ShapeScheduleMatches(shape, equivalentShape))
					{
						return false;
					}
				}
			}

			return true;
		}

		bool ShapeScheduleMatches(BMNCNShape shape, BMNCNShape equivalentShape)
		{
			return shape.EarliestFinishHours == equivalentShape.EarliestFinishHours
				&& shape.EarliestStartHours == equivalentShape.EarliestStartHours
				&& shape.LatestStartHours == equivalentShape.LatestStartHours
				&& shape.LatestFinishHours == equivalentShape.LatestFinishHours
				&& shape.IsCriticalPath == equivalentShape.IsCriticalPath
				&& shape.FloatHours == equivalentShape.FloatHours;
		}

		bool ScheduleEquals(ScheduleNode schedule, ScheduleNode equivalentSchedule)
		{
			return schedule.EarliestFinishHours == equivalentSchedule.EarliestFinishHours
				&& schedule.EarliestStartHours == equivalentSchedule.EarliestStartHours
				&& schedule.LatestStartHours == equivalentSchedule.LatestStartHours
				&& schedule.LatestFinishHours == equivalentSchedule.LatestFinishHours
				&& schedule.IsCriticalPath == equivalentSchedule.IsCriticalPath
				&& schedule.FloatHours == equivalentSchedule.FloatHours;
		}

		#endregion

		#endregion

		#region SimpleTestNetwork

		class SimpleTestNetwork : NonPersistentBusinessObject
		{
			public SimpleTestNetwork(BusinessObjectFactory factory)
				: base(factory)
			{
				SetupSimpleTestNetwork();
			}

			public IJobNetwork Network { get; set; }

			public BMNCNShape Shape1 { get; set; }
			public BMNCNShape Shape2_cc { get; set; }
			public BMNCNShape Shape2_noncc { get; set; }
			public BMNCNShape Shape3 { get; set; }

			void SetupSimpleTestNetwork()
			{
				var diagram = NetworkTestCase.CreateDiagram(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory));
				var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
				Network = networkViewModel.GetJobNetwork();
				Network.SwitchToScaled();

				Shape1 = networkViewModel.CreateNewShape(diagram).Shape;
				Shape2_cc = networkViewModel.CreateNewShape(diagram).Shape;
				Shape2_noncc = networkViewModel.CreateNewShape(diagram).Shape;
				Shape3 = networkViewModel.CreateNewShape(diagram).Shape;

				Shape1.MakeVisiblePrerequisiteOf(Shape2_cc, diagram);
				Shape1.MakeVisiblePrerequisiteOf(Shape2_noncc, diagram);

				Shape2_cc.MakeVisiblePrerequisiteOf(Shape3, diagram);
				Shape2_noncc.MakeVisiblePrerequisiteOf(Shape3, diagram);

				Shape1.BNS_Name = "s1";
				Shape2_cc.BNS_Name = "s2cc";
				Shape2_noncc.BNS_Name = "s2n";
				Shape3.BNS_Name = "s3";

				Shape1.ExplicitDurationMinutes = 60;
				Shape2_cc.ExplicitDurationMinutes = 120;
				Shape2_noncc.ExplicitDurationMinutes = 60;
				Shape3.ExplicitDurationMinutes = 60;

				Factory.Save();
			}
		}

		#endregion

		#region IncompatibleNetwork

		public JobNetwork IncompatibleNetwork
		{
			get
			{
				if (incompatibleNetwork == null)
				{
					incompatibleNetwork = NetworkTestCase.CreateNetwork(NetworkTestCase.CreateDiagram(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory)));
				}
				return incompatibleNetwork;
			}
		}
		JobNetwork incompatibleNetwork;

		#endregion
	}
}
