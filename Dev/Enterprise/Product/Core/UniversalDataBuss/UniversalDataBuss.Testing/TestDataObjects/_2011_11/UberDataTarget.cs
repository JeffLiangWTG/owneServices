using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects._2011_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), NamespaceSpecific(UniversalXmlInfo.Namespace_2011_11)]
	public class UberDataTarget : IDataTargetDataObject
	{
		[MaxLength(35), Mandatory, CandidateKey]
		public ZString? Type { get; set; }
		[MaxLength(35)]
		public ZString? Key { get; set; }
		public UberOrganization Owner { get; set; }

		IOrganizationAddress IDataTargetDataObject.Owner
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
	}
}

