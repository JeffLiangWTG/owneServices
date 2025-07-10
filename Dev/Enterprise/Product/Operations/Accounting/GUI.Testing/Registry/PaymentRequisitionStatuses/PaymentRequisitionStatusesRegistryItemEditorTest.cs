using System;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(PaymentRequisitionStatusesRegistryItemEditor))]
	class PaymentRequisitionStatusesRegistryItemEditorTest : CodeDescriptionBoolWithExtraBoolRegistryItemEditorTest
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new PaymentRequisitionStatusesRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), null);
		}
	}
}
