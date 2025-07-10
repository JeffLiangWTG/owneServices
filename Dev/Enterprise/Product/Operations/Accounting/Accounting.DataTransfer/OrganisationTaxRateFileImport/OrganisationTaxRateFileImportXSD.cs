using System.Collections.Generic;
using System.Xml.Serialization;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer
{
	class OrganisationTaxRateFileImportXSD : List<OrganisationTaxRateFileImportLine>, IValueObject
	{
		#region IValueObject Members

		[XmlIgnore]
		bool IValueObject.IsSpecified => true;

		[XmlIgnore]
		bool IValueObject.ShouldCreateElementForEmptyValue { get; set; }

		#endregion
	}
}
