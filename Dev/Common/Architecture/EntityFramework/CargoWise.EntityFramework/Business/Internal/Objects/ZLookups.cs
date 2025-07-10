using CargoWise.Common;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	[IncludeOnlyNamedPropertiesForPropertyDescriptorReflection]
	public abstract class ZLookups : ZCustomTypeDescriptor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "simple identifier")]
		public const string LookupsBindingMember = "Lookups";

		protected ZLookups(BusinessObject parent)
		{
			this.Parent = parent;
		}

		protected readonly BusinessObject Parent;

		protected virtual BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}

		protected void ReportNeedsToBeOverriddenError(string propertyName)
		{
			ErrorReporter.ReportOnce(GetType().FullName + propertyName,
				"OrgHeader Collections should be overridden with a specific Org collection (eg. ConsigneeCollection)." + System.Environment.NewLine +
				"Alternately, override and add a filter." + System.Environment.NewLine +
				"Override " + propertyName + " and return the correct collection." + System.Environment.NewLine +
				"This message will continue to appear until you stop calling base.");
		}
	}
}
