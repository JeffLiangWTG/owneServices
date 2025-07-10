using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentEngine
{
	public class FilterBuilderDocumenter : IReportDocumenter
	{
		public FilterBuilderDocumenter(ZString useage, ZString explanation, List<string> supportedProperties, bool supportLookup)
			: this(useage, explanation, supportedProperties)
		{
			this.supportLookup = supportLookup;
		}

		public FilterBuilderDocumenter(ZString useage, ZString explanation, List<string> supportedProperties)
		{
			this.useage = useage;
			this.explanation = explanation;
			this.supportedProperties = supportedProperties;
		}
		readonly ZString useage;
		readonly ZString explanation;
		readonly List<string> supportedProperties;
		readonly bool supportLookup;
		List<ValueProviderDocumenter> valueProviderDocumenters;

		public ZString DefaultOptions { get; set; }

		public ZString Useage
		{
			get { return useage; }
		}

		public ZString Explanation
		{
			get { return string.IsNullOrEmpty(DefaultOptions) ? explanation : ZString.Join(System.Environment.NewLine, new ZString[] { explanation, DefaultOptions }); }
		}

		public List<string> SupportedProperties
		{
			get
			{
				return supportedProperties;
			}
		}

		public bool SupportLookup
		{
			get { return supportLookup; }
		}

		public List<ValueProviderDocumenter> ValueProviderDocumenters
		{
			get
			{
				return valueProviderDocumenters;
			}
			set
			{
				valueProviderDocumenters = value;
			}
		}
	}
}
