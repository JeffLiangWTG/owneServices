using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.XmlMapping
{
	[Immutable]
	[ImmutableObject(true)]
	public class DepartmentActivityXmlMapping : EnterpriseCodeExternalCodeMappings
	{
		DepartmentActivityXmlMapping()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Constants.DepartmentActivityTypes.Cartage, nameof(Xsd.DepartmentActivity.Cartage));
			yield return new Mapping(Constants.DepartmentActivityTypes.Customs, nameof(Xsd.DepartmentActivity.Customs));
			yield return new Mapping(Constants.DepartmentActivityTypes.DepotCFS, nameof(Xsd.DepartmentActivity.DepotCFS));
			yield return new Mapping(Constants.DepartmentActivityTypes.Forwarding, nameof(Xsd.DepartmentActivity.Forwarding));
			yield return new Mapping(Constants.DepartmentActivityTypes.Linehaul, nameof(Xsd.DepartmentActivity.Linehaul));
			yield return new Mapping(Constants.DepartmentActivityTypes.Miscellaneous, nameof(Xsd.DepartmentActivity.Miscellaneous));
			yield return new Mapping(Constants.DepartmentActivityTypes.Warehouse, nameof(Xsd.DepartmentActivity.Warehouse));
			yield return new Mapping(Constants.DepartmentActivityTypes.Shipping, nameof(Xsd.DepartmentActivity.Shipping));
			yield return new Mapping(Constants.DepartmentActivityTypes.Gateway, nameof(Xsd.DepartmentActivity.Gateway));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override string Name
		{
			get { return "Department Activity"; }
		}

		public new Xsd.DepartmentActivity GetExternalCode(string enterpriseConstant, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseConstant, Xsd.DepartmentActivity.NotDefined, errorContext, notify);
		}

		public static readonly DepartmentActivityXmlMapping Instance = new DepartmentActivityXmlMapping();

		#region NameForTesting
#if DEBUG

		public string NameForTesting
		{
			get { return Name; }
		}

#endif
		#endregion
	}
}
