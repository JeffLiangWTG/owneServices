using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class SetStatusAction : JobNetworkAction
	{
		public SetStatusAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return new NetworkActionAccessibility(shape.CanSetStatus(),
				shape,
				() => Res.GetString("CED960CE-8FCC-46C5-93F5-CBDC8227937D", "Should be a shape or diagram not linked to a job or workflow."));
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("24c57311-2ba4-4007-9a70-c51f213822b3", "Set Status");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("16af4a69-de06-48d2-855e-25356fc164da", "Marks this shape with a status");
		}

		protected override string IconName => (NoResString)"Status"; // it's an argument

		protected override IEnumerable<INetworkAction> GetChildActionsCore(BMNCNShape shape)
		{
			return setStatusActions ?? (setStatusActions = GetSetStatusActions().ToArray());
		}

		IEnumerable<INetworkAction> setStatusActions;

		IEnumerable<INetworkAction> GetSetStatusActions()
		{
			foreach (var pair in GetOrderedStatuses())
			{
				yield return new ChildStatusAction(NetworkViewModel, pair, ShouldUpdateOnNetworkEvents);
			}
		}

		public static IEnumerable<CodeDescriptionPair> GetOrderedStatuses()
		{
			var set = new ShapeStatusList().Cast<CodeDescriptionPair>().ToDictionary(c => c.Code);
			var order = new[]
			{
				ShapeStatusList.Codes.Open,
				ShapeStatusList.Codes.Assigned,
				ShapeStatusList.Codes.Working,
				ShapeStatusList.Codes.Suspended,
				ShapeStatusList.Codes.Closed,
				ShapeStatusList.Codes.Cancelled,
			};

			foreach (var code in order)
			{
				yield return set[code];
			}
		}

		public class ChildStatusAction : JobNetworkAction
		{
			public ChildStatusAction(INetworkViewModel networkViewModel, CodeDescriptionPair pair, bool shouldUpdateOnNetworkEvents = true)
				: base(networkViewModel, new SetStatusActionExecutionStrategy(), shouldUpdateOnNetworkEvents: shouldUpdateOnNetworkEvents)
			{
				statusPair = pair;
			}

			readonly CodeDescriptionPair statusPair;

			IEnumerable<BMNCNShape> GetShapesToExecute() => ((SetStatusActionExecutionStrategy)ExecutionStrategy).GetEntitiesToExecute(this).Select(e => e.AsShape()).ToArray();

			protected override object DefineExecutionSessionData() => IsActivated();

			bool ShouldChangeStatusToUnknown => (bool)ExecutionSessionData;

			protected override void ExecuteForShape(BMNCNShape shape)
			{
				using (ActiveBusinessObjectCollection.DelayListChangedEvents(shape.Factory))
				{
					var code = ShouldChangeStatusToUnknown ? ShapeStatusList.Codes.Unknown : statusPair.Code;

					shape.BNS_Status = code;

					var shapes = shape.Descendants(new BMNCNShapeDescendantsStrategy()).Where(s => s.CanSetStatus() && s.BNS_Status != code).ToArray();

					if (shapes.Length > 0)
					{
						var message = ShouldChangeStatusToUnknown ? Res.GetString("6b219d26-005a-4236-8418-8ac824832239", "Would you like to clear the status of all unlinked child shapes of {0}?", shape.Name)
							: Res.GetString("334f9d7c-8813-4e46-bb05-dea161b283e9", "Would you also like to mark all child unlinked shapes of {0} as {1}?", shape.Name, statusPair.Description);
						var heading = ShouldChangeStatusToUnknown ? Res.GetString("644d75c2-330f-42a0-b2bc-a7cffc25aae8", "Clear children status?")
							: Res.GetString("2cf060e1-ef35-4957-8131-f7843ed94f43", "Set children as {0}?", statusPair.Description);
						if (UserInteractionImplementor.HasUserAnsweredYes(message, heading))
						{
							foreach (var childShape in shapes)
							{
								childShape.BNS_Status = code;
							}
						}
					}
				}
				Network.Refresh(RefreshType.EntityStatusChanged, Network.Entities.ToArray());
			}

			protected override void ExecuteForShapeNetworkEntity(ShapeNetworkEntity shapeEntity)
			{
				if (shapeEntity != null)
				{
					var levelingRules = shapeEntity.Network.DiagramEntity.LevelingRules;
					if (levelingRules != null && levelingRules.Any())
					{
						shapeEntity.UpdateLevelingRuleViolations();
						Network.Refresh(RefreshType.EntityStatusChanged, Network.Entities.ToArray());
					}
				}
			}

			protected override ResourceString GetDefaultNameCore()
			{
				return ResString.GetMultilingualString("27F1D3B1-43EE-4CE1-BF0B-463EAC5DCD05", "{0}", statusPair.CodeAndDescription);
			}

			protected override ResourceString GetDefaultDescriptionCore()
			{
				return ResString.GetMultilingualString("0aa51aa6-dba2-4763-bb34-3c240f000a44", "Set status to {0}.", statusPair.Description);
			}

			protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
			{
				return NetworkActionAccessibility.Allowed;
			}

			protected override bool IsActivatedCore(BMNCNShape shape)
			{
				return GetShapesToExecute().All(s => s.BNS_Status == statusPair.Code);
			}

			protected override string IconName => $"Statuses_{statusPair.Code}"; // resource name
		}
	}
}
