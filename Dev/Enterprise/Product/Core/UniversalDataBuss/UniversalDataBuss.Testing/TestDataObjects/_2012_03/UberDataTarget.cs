using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects._2012_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), NamespaceSpecific(UniversalXmlInfo.Namespace_2012_11)]
	public class UberDataTarget : IDataTargetDataObject
	{
		public UberReferenceType? Type { get; set; }
		[MaxLength(20)]
		public ZString? Key { get; set; }
		public UberOrganization Owner { get; set; }

		ZString? IDataTargetDataObject.Type
		{
			get { return Type.HasValue ? Type.Value.ToString() : null; }
			set { throw new NotImplementedException(); }
		}

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

