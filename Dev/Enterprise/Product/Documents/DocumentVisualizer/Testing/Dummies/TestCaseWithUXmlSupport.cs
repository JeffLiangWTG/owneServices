using System;
using System.Collections;
using System.Reactive.Disposables;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	abstract class TestCaseWithUXmlSupport : TestCaseWithFactory
	{
		#region Implementation

		protected IDisposable DataContextManagersSubstitution()
		{
			var activateDescriptor = DummyWorkflowDescriptor.Instance;

			var contextManagers = new Hashtable
			{
				{ nameof(DataContextType.DummyBusinessObject), new TestObjectHandle(new DummyWithUXmlSupportDataContextManager()) },
				{ nameof(DataContextType.DocumentData), new TestObjectHandle(new VisualizerDocumentDataContextManager()) }
			};

			var universalDataContextManagersSubstitution = ObjectFactory.Substitute("UniversalDataContextManagers", contextManagers);

			return Disposable.Create(() =>
			{
				universalDataContextManagersSubstitution.Dispose();
			});
		}

		#endregion
	}
}