using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using CargoWise.Windows.UI.Design;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KTabControlTest : ControlTestCase<KTabControl>
	{
		public void TestCreateHandleOnDisposedObjectShouldNotThrowException()
		{
			using (var kTabControl = new KTabControl())
			{
				var handle = kTabControl.Handle;
				AssertNotNull("Handle should not be null", handle);
				kTabControl.Dispose();
				Assert("Object should be disposed", kTabControl.IsDisposed);
				AssertNoExceptionThrown(() => handle = kTabControl.Handle);
			}
		}

#if !WINZOR // Don't need to test WinForms designer in Winzor
		public void TestDesigner()
		{
			DesignerAttribute attr = GetDesignerAttributeJustLikeVS(typeof(KTabControl));
			AssertEquals(Type.GetType(DesignerTypes.KTabControlDesigner), Type.GetType(attr.DesignerTypeName));
		}

		static DesignerAttribute GetDesignerAttributeJustLikeVS(Type type)
		{
			foreach (Attribute attr in TypeDescriptor.GetAttributes(type))
			{
				DesignerAttribute designerAttr = attr as DesignerAttribute;

				// Normalize the DesignerBaseTypeName by stripping off assembly info.
				var normalizedBaseTypeName = typeof(IDesigner).Assembly.GetType(designerAttr.DesignerBaseTypeName.Split(',')[0].Trim());
				if (designerAttr != null && normalizedBaseTypeName == typeof(IDesigner))
				{
					return designerAttr;
				}
			}
			return null;
		}
#endif
	}
}
