using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class TestDynamicNetworkAction : DynamicNetworkAction
	{
		public TestDynamicNetworkAction(NetworkViewModel networkViewModel, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, shouldUpdateOnNetworkEvents: shouldUpdateOnNetworkEvents)
		{
		}

		public int NameRequestsCounter { get; private set; }
		public int DescriptionRequestsCounter { get; private set; }
		public int IsActivatedRequestsCounter { get; private set; }
		public int ChildActionsRequestsCounter { get; private set; }
		public int IsApplicableRequestsCounter { get; private set; }
		public int IsEnabledRequestsCounter { get; private set; }
		public int IconRequestsCounter { get; private set; }

		public ResourceString ReturnValueForGetNameCore { get; set; } = ResString.GetMultilingualString("TestDynamicNetworkAction - Name", "Name");
		public ResourceString ReturnValueForGetDescriptionCore { get; set; } = ResString.GetMultilingualString("TestDynamicNetworkAction - Description", "Description");
		public bool ReturnValueForIsActivatedCore { get; set; } = true;
		public IEnumerable<INetworkAction> ReturnValueForGetChildActionsCore { get; set; } = new INetworkAction[] { new StaticNetworkAction() };
		public INetworkActionAccessibility ReturnValueForIsApplicableToEntityCore { get; set; } = NetworkActionAccessibility.Allowed;
		public INetworkActionAccessibility ReturnValueForIsEnabledForEntityCore { get; set; } = NetworkActionAccessibility.Allowed;

		public void ResetCounters()
		{
			NameRequestsCounter = 0;
			DescriptionRequestsCounter = 0;
			IsActivatedRequestsCounter = 0;
			ChildActionsRequestsCounter = 0;
			IsApplicableRequestsCounter = 0;
			IsEnabledRequestsCounter = 0;
			IconRequestsCounter = 0;
		}

		protected override ResourceString GetDefaultNameCore() => null;

		protected override ResourceString GetDefaultDescriptionCore() => null;

		protected override ResourceString GetNameCore(INetworkEntity activeEntity)
		{
			NameRequestsCounter++;
			return ReturnValueForGetNameCore;
		}

		protected override ResourceString GetDescriptionCore(INetworkEntity activeEntity)
		{
			DescriptionRequestsCounter++;
			return ReturnValueForGetDescriptionCore;
		}

		protected override bool IsActivatedCore(INetworkEntity activeEntity)
		{
			IsActivatedRequestsCounter++;
			return ReturnValueForIsActivatedCore;
		}

		protected override IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity)
		{
			ChildActionsRequestsCounter++;
			return ReturnValueForGetChildActionsCore;
		}

		protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity)
		{
			IsApplicableRequestsCounter++;
			return ReturnValueForIsApplicableToEntityCore;
		}

		protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity)
		{
			IsEnabledRequestsCounter++;
			return ReturnValueForIsEnabledForEntityCore;
		}

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			throw new System.NotImplementedException();
		}

		protected override string IconName
		{
			get
			{
				IconRequestsCounter++;
				return null;
			}
		}
	}
}
