using System;
using System.Threading;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.TNT
{
	/// <summary>
	/// Summary description for TNTStringToBusinessObjectFieldConverter.
	/// </summary>
	public class TNTStringToBusinessObjectFieldConverter : StringToBusinessObjectFieldConverter
	{
		protected TNTStringToBusinessObjectFieldConverter(ZGuid mappingOrgPK)
			: base(mappingOrgPK)
		{
		}

		public static TNTStringToBusinessObjectFieldConverter Instance
		{
			get { return fInstance ?? (fInstance = new TNTStringToBusinessObjectFieldConverter(GlbCompany.CurrentCompany.GC_OH_OrgProxy)); }
		}
		[ThreadStatic]
		static TNTStringToBusinessObjectFieldConverter fInstance;

		protected override ZDateTime ParseDateTime(string valueAsString)
		{
			return TNTStringToBusinessObjectFieldConverter.ParseADateTime(valueAsString);
		}

		public static ZDateTime ParseADateTime(string valueAsString)
		{
			ZDateTime result;
			if (string.IsNullOrWhiteSpace(valueAsString))
			{
				result = ZDateTime.Empty;
			}
			else
			{
				result = DateTime.ParseExact(valueAsString, "ddMMyy", Thread.CurrentThread.CurrentCulture);
			}
			return result;
		}
	}
}
