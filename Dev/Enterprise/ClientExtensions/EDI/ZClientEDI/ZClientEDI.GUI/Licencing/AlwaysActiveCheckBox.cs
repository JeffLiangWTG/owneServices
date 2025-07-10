using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public class AlwaysActiveCheckBox : ZCheckBox
	{
		protected override ControlReadOnlyPropertyHelper ReadOnlyForBindingProperty
		{
			get
			{
				if (readOnlyForBindingProperty == null)
				{
					readOnlyForBindingProperty = new ControlReadOnlyPropertyHelper(
							this,
							() => ReadOnly,
							(value) => ReadOnly = false);
				}
				return readOnlyForBindingProperty;
			}
		}

		ControlReadOnlyPropertyHelper readOnlyForBindingProperty;
	}
}
