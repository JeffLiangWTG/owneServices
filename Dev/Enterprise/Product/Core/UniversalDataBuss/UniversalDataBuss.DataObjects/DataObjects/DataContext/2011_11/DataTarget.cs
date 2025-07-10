using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), NamespaceSpecific(UniversalXmlInfo.Namespace_2011_11)]
	public class DataTarget : IDataTargetDataObject
	{
		[MaxLength(35), Mandatory, CandidateKey]
		public ZString? Type { get; set; }
		[MaxLength(300)]
		public ZString? Key { get; set; }
		public OrganizationAddress Owner { get; set; }

		IOrganizationAddress IDataTargetDataObject.Owner
		{
			get { return Owner; }
			set { Owner = (OrganizationAddress)value; }
		}
	}
}

