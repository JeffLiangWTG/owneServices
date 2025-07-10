using System;
using System.ComponentModel;
using System.Xml.Serialization;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Core
{
	[TypeConverter(typeof(ZMultilingualTypeConverter))]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public abstract class ZMultilingual : IMultilingual, IZType, IZTypeInternals
	{
		object IMultilingual.GetLocalizedValue(string language)
		{
			return GetLocalizedValue(language);
		}

		object IMultilingual.GetUnresolvedValue()
		{
			return GetUnresolvedValue();
		}

		public abstract IZType GetLocalizedValue(string language);
		public abstract IZType GetUnresolvedValue();

		#region IZType Members

		[XmlIgnore]
		public ZDataType DataType
		{
			get { return GetUnresolvedValue().DataType; }
		}

		[XmlIgnore]
		public Type BaseDataType
		{
			get { return typeof(string); }
		}

		[XmlIgnore]
		public IZType Default
		{
			get { return GetUnresolvedValue().Default; }
		}

		[XmlIgnore]
		public bool IsDefault
		{
			get { return GetUnresolvedValue().IsDefault; }
		}

		[XmlIgnore]
		public bool IsEmpty
		{
			get { return GetUnresolvedValue().IsEmpty; }
		}

		[XmlIgnore]
		public bool IsValid
		{
			get { return GetUnresolvedValue().IsValid; }
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			ZMultilingual other = obj as ZMultilingual;
			if (other != null)
			{
				return GetLocalizedValue(Res.CurrentLanguage).CompareTo(other.GetLocalizedValue(Res.CurrentLanguage));
			}
			else
			{
				return GetLocalizedValue(Res.CurrentLanguage).CompareTo(obj);
			}
		}

		#endregion

		#region IZTypeInternals Members

		public object GetValueForLogicalDataLayer(bool isNullable)
		{
			return GetLocalizedValue(Res.CurrentLanguage);
		}

		#endregion
	}
}
