using System;
using System.Collections;
using CargoWise.Application;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class OrganisationCountrySpecificDataTransferTool
	{
		#region Constructors

		protected OrganisationCountrySpecificDataTransferTool()
		{
		}

		public static OrganisationCountrySpecificDataTransferTool New()
		{
			return New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public static OrganisationCountrySpecificDataTransferTool New(string countryCode)
		{
			return GetCountrySpecificDataTool(countryCode) ?? new OrganisationCountrySpecificDataTransferTool();
		}

		static OrganisationCountrySpecificDataTransferTool GetCountrySpecificDataTool(string countryCode)
		{
			OrganisationCountrySpecificDataTransferTool result = null;

			Hashtable types = (Hashtable)ObjectFactory.Get("OrganisationCountrySpecificDataTransferToolList");

			if (types.Contains(countryCode))
			{
				result = New(((Type)types[countryCode]));
			}
			return result;
		}

		static OrganisationCountrySpecificDataTransferTool New(Type type)
		{
			return (OrganisationCountrySpecificDataTransferTool)Activator.CreateInstance(type);
		}

		#endregion

		public virtual void ImportData(OrgHeader organisation, Xsd.OrganisationDetail xsdOrgDetails, IValueObjectImportContext context)
		{
		}

		public virtual void ExportData(OrgHeader organisation, Xsd.OrganisationDetail xsdOrgDetails, IValueObjectExportContext context)
		{
		}
	}
}
