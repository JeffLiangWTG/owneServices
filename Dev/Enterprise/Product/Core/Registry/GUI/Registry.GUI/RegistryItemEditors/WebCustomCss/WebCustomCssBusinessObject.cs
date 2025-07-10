using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	class WebCustomsCssCollectionWrapper : NonPersistentBusinessObject
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public WebCustomsCssCollectionWrapper(string[] urls, WebTrackerCustomCss[] values)
		{
			var suppliedValues = values.Select(v => new WebCustomCssBusinessObject { Url = v.Url, Data = v.Data });
			var defaultValues = urls.Append(string.Empty).Select(url => new WebCustomCssBusinessObject { Url = url });

			Collection.AddRange(suppliedValues.Concat(defaultValues).DistinctBy(bizo => bizo.Url).OrderBy(bizo => bizo.Url));

			RegisterEditableChildObject(Collection);
		}

		public WebCustomCssBusinessObjectCollection Collection { get; } = new WebCustomCssBusinessObjectCollection();

		public WebTrackerCustomCss[] ToWebTrackerCustomCssArray()
			=> Collection.Cast<WebCustomCssBusinessObject>()
					.Where(v => !v.IsEmpty)
					.Select(v => v.ToWebTrackerCustomCss())
					.ToArray();
	}

	class WebCustomCssBusinessObjectCollection : NonPersistentBusinessObjectCollection<WebCustomCssBusinessObject>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject() => new WebCustomCssBusinessObject();

		public WebCustomCssBusinessObject this[string url] => this.Cast<WebCustomCssBusinessObject>().FirstOrDefault(o => o.Url == url);
	}

	[DebuggerDisplay("[Theme:{Url}, Empty: {IsEmpty}]")]
	class WebCustomCssBusinessObject : NonPersistentBusinessObject
	{
		public string DisplayValue
			=> string.IsNullOrEmpty(Url) ? Res.GetString("d5324f0e-e9fc-4076-987c-ba41c7403c48", "All URLs") : Url.ToString();

		public ZString Url
		{
			get => url;
			set => SetNonPersistentPropertyValue(UrlInfo, ref url, value);
		}
		ZString url;

		public ZPropertyInfo UrlInfo => GetZPropertyInfo(nameof(Url));

		public ZString Data
		{
			get => data;
			set => SetNonPersistentPropertyValue(DataInfo, ref data, value);
		}
		ZString data = DefaultDataValue;

		public ZPropertyInfo DataInfo => GetZPropertyInfo(nameof(Data));

		public WebTrackerCustomCss ToWebTrackerCustomCss() => new WebTrackerCustomCss(Url.ToString(), data.ToString());

		public bool IsEmpty
			=> string.IsNullOrWhiteSpace(Data) || Data.Trim() == DefaultDataValue.Trim();

		public const string DefaultDataValue = WebCustomCssControl.CssImportStatement;
	}
}
