using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.Native.Business.XmlSerializers")]
	[XmlInclude(typeof(HeaderData_Versioned_Native))]
	public abstract class HeaderData
	{
		protected HeaderData()
		{
			OwnerCode = string.Empty;
			EnableCodeMapping = true;
		}

		public string OwnerCode { get; set; }

		[XmlIgnore]
		public bool EnableCodeMapping { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		[XmlElement("EnableCodeMapping")]
		public string EnableCodeMappingSerialize
		{
			get { return EnableCodeMapping.ToString().ToLower(); }
			set
			{
				if (value.Trim().IsNullOrEmpty())
				{
					value = "false";
				}

				if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					EnableCodeMapping = true;
				}
				else if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
				{
					EnableCodeMapping = false;
				}
				else
				{
					try
					{
						EnableCodeMapping = XmlConvert.ToBoolean(value);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						throw new NativeXMLUserVisibleException(FormattableString.Invariant($"EnableCodeMapping set to invalid value [{value}]."));
					}
				}
			}
		}

		public DataContextWrapper DataContext { get; set; }

		[XmlIgnore]
		public Guid TargetCompanyPK { get; set; }

		public List<MessageNumberWrapper> MessageNumberCollection { get; set; }

		public static HeaderData New(string ownerCode, DataContextWrapper dataContext, List<MessageNumberWrapper> messageNumberCollection)
		{
			switch (ReferenceDataXMLForSerialize.Instance.NameSpace)
			{
				case ReferenceDataXMLForDeSerialize.NameSpace_Universal:
					return new HeaderData_Universal { OwnerCode = ownerCode, DataContext = dataContext, MessageNumberCollection = messageNumberCollection };
				case ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native:
					return new HeaderData_Unversioned_Native { OwnerCode = ownerCode, DataContext = dataContext, MessageNumberCollection = messageNumberCollection };
				default:
					return new HeaderData_Versioned_Native { OwnerCode = ownerCode, DataContext = dataContext, MessageNumberCollection = messageNumberCollection };
			}
		}
	}
}
