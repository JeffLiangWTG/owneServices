using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	[DefaultField("Useage")]
	public class ReportFilterBuilderWrapper : GenericWrapper
	{
		public ReportFilterBuilderWrapper(IReportDocumenter reportDocumenter, BusinessObjectFactory factory)
			: base(null, factory)
		{
			if (reportDocumenter == null)
			{
				ReportDocumenter = new FilterBuilderDocumenter("", "", new List<string>());
				ReportDocumenter.ValueProviderDocumenters = new List<ValueProviderDocumenter>();
			}
			else
			{
				ReportDocumenter = reportDocumenter;
			}
		}

		readonly IReportDocumenter ReportDocumenter;

		public ZString Useage
		{
			get { return ReportDocumenter.Useage; }
		}

		public ZString Explanation
		{
			get { return ReportDocumenter.Explanation; }
		}

		public bool SupportLookup
		{
			get { return ReportDocumenter.SupportLookup; }
		}

		public ZString SupportedProperties
		{
			get { return string.Join(System.Environment.NewLine, ReportDocumenter.SupportedProperties); }
		}

		public ZString ValueProviderDocumenters
		{
			get
			{
				if (ReportDocumenter.ValueProviderDocumenters == null)
				{
					return "";
				}

				var list = new List<string>();
				foreach (var documenter in ReportDocumenter.ValueProviderDocumenters)
				{
					list.Add(string.Format(CultureInfo.InvariantCulture, @"{0}: {1}", documenter.Useage, documenter.Explanation));
				}
				return string.Join(System.Environment.NewLine, list);
			}
		}
	}
}
