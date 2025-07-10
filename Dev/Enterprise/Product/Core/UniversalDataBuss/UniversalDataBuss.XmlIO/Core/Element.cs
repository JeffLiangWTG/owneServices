using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;

namespace Enterprise.UniversalDataBuss.XmlIO
{
	abstract class Element
	{
		protected Element(PropertyInfo propertyInfo, ElementProcessor processor)
			: this(propertyInfo.Name, processor)
		{
			this.PropertyInfo = propertyInfo;
		}

		protected Element(string elementName, ElementProcessor processor)
		{
			this.ElementName = elementName;
			this.Processor = Argument.NotNull(processor, "ElementProcessor processor");
		}

		protected readonly ElementProcessor Processor;
		internal readonly string ElementName;
		internal readonly PropertyInfo PropertyInfo;

		internal string MinOccurs
		{
			get
			{
				return PropertyInfo != null
					? PropertyInfo.GetCustomAttributes(typeof(MandatoryAttribute), false).Length > 0 ? "1" : "0"
					: null;
			}
		}

		internal abstract PlacingWithinXml DefaultPlacing { get; }

		internal string KeyForSorting
		{
			get { return Processor.GetCachedPlacementManager(this).KeyForSorting; }
		}

		public PlacingWithinXml ElementPlacing
		{
			get { return Processor.GetCachedPlacementManager(this).ElementPlacing; }
		}

		protected string Namespace
		{
			get { return Processor.Namespace; }
		}
	}
}

