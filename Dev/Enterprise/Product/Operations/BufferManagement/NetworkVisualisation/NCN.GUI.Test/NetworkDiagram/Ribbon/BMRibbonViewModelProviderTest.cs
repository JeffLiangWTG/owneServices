using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	public class BMRibbonViewModelProviderTest : TransactionedTestCase
	{
		public void TestShouldReturnRibbonViewModel_WhenEnabledInRegistry()
		{
			BMSRegistry.Instance.NCNRibbonEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var diagram = NetworkTestCase.CreateDiagram(new BusinessObjectFactory());
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			AssertNotNull(new BMRibbonViewModelProvider().GetRibbonViewModel(networkViewModel, null));
		}

		public void TestShouldReturnNullAsRibbonViewModel_WhenDisabledInRegistry()
		{
			BMSRegistry.Instance.NCNRibbonEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var diagram = NetworkTestCase.CreateDiagram(new BusinessObjectFactory());
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			AssertNull(new BMRibbonViewModelProvider().GetRibbonViewModel(networkViewModel, null));
		}
	}
}
