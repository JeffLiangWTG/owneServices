using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public class DataContextWrapper
	{
		DataContextWrapper()
		{
		}

		public static DataContextWrapper New(IDataContextDataObject dataContextInfo)
		{
			var dataObject = dataContextInfo as UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
			if (dataObject != null)
			{
				var result = new DataContextWrapper();
				if (dataObject.DataSourceCollection != null)
				{
					var dataSourceCollection = new List<DataContextReferenceWrapper>();
					foreach (var dataSource in dataObject.DataSourceCollection)
					{
						dataSourceCollection.Add(DataContextReferenceWrapper.New(dataSource));
					}

					result.DataSourceCollection = dataSourceCollection;
				}

				if (dataObject.CodesMappedToTarget.HasValue && dataObject.CodesMappedToTarget.Value)
				{
					result.CodesMappedToTarget = true;
				}

				result.Company = CompanyWrapper.New(dataObject.Company);
				result.EnterpriseID = dataObject.EnterpriseID;
				result.ServerID = dataObject.ServerID;
				result.ActionPurpose = CodeDescriptionPairWrapper.New(dataObject.ActionPurpose);
				result.EventReference = dataObject.EventReference;
				result.EventType = CodeDescriptionPairWrapper.New(dataObject.EventType);
				result.EventUser = CodeNamePairWrapper.New(dataObject.EventUser);
				result.EventBranch = CodeNamePairWrapper.New(dataObject.EventBranch);
				result.EventDepartment = CodeNamePairWrapper.New(dataObject.EventDepartment);
				result.TriggerDate = ConvertToDateTime(dataObject.TriggerDate);
				result.TriggerDescription = dataObject.TriggerDescription;
				result.TriggerReference = dataObject.TriggerReference;
				result.TriggerCount = dataObject.TriggerCount;

				if (dataObject.TriggerType.HasValue)
				{
					result.TriggerType = dataObject.TriggerType.Value.ToString();
				}

				var recipientRoles = dataObject.RecipientRoleCollection;

				if (recipientRoles != null && recipientRoles.Any())
				{
					result.RecipientRoleCollection = new List<RecipientRoleWrapper>();

					foreach (var recipientRole in recipientRoles)
					{
						if (recipientRole != null && recipientRole.Code.HasValue)
						{
							ZString recipientRoleDescription = recipientRole.Description ?? ZString.Empty;
							result.RecipientRoleCollection.Add(RecipientRoleWrapper.New(recipientRole.Code.Value.ToString(), recipientRoleDescription));
						}
					}
				}
				result.Timestamp = dataObject.Timestamp;

				return result;
			}
			var xmlDataProvider = ObjectFactory.Get<IXMLDataProvider>();
			if (!xmlDataProvider.IgnoreTimestamp)
			{
				var result = new DataContextWrapper();
				result.Timestamp = xmlDataProvider.Timestamp;
				return result;
			}
			return null;
		}

		static DateTime? ConvertToDateTime(ZDateTimeOffset? nullableZDateTime)
		{
			if (nullableZDateTime.HasValue)
			{
				var value = nullableZDateTime.Value;
				if (value.IsValid)
				{
					return new ZDateTime(value.ToZDateTime(), DateTimeKind.Local).ToDateTime();
				}
			}

			return null;
		}

		public List<DataContextReferenceWrapper> DataSourceCollection { get; set; }
		public CodeDescriptionPairWrapper ActionPurpose { get; set; }
		public bool? CodesMappedToTarget { get; set; }
		public CompanyWrapper Company { get; set; }
		public string EnterpriseID { get; set; }
		public string EventReference { get; set; }
		public CodeDescriptionPairWrapper EventType { get; set; }
		public CodeNamePairWrapper EventUser { get; set; }
		public CodeNamePairWrapper EventBranch { get; set; }
		public CodeNamePairWrapper EventDepartment { get; set; }
		public string ServerID { get; set; }
		public long? Timestamp { get; set; }
		public int? TriggerCount { get; set; }
		public DateTime? TriggerDate { get; set; }
		public string TriggerDescription { get; set; }
		public string TriggerReference { get; set; }
		public string TriggerType { get; set; }
		public List<RecipientRoleWrapper> RecipientRoleCollection { get; set; }

		[XmlIgnore]
		public bool CodesMappedToTargetSpecified { get { return CodesMappedToTarget.HasValue; } }
		[XmlIgnore]
		public bool TimestampSpecified => Timestamp.HasValue;
		[XmlIgnore]
		public bool TriggerCountSpecified { get { return TriggerCount.HasValue; } }
		[XmlIgnore]
		public bool TriggerDateSpecified { get { return TriggerDate.HasValue; } }
		[XmlIgnore]
		public bool TriggerDescriptionSpecified { get { return !string.IsNullOrEmpty(TriggerDescription); } }
		[XmlIgnore]
		public bool TriggerReferenceSpecified { get { return !string.IsNullOrEmpty(TriggerReference); } }

		[XmlType("DataSource")]
		public class DataContextReferenceWrapper
		{
			public string Type { get; set; }
			public string Key { get; set; }

			internal static DataContextReferenceWrapper New(IDataSourceDataObject reference)
			{
				return reference == null ? null : new DataContextReferenceWrapper
				{
					Key = reference.Key,
					Type = reference.Type,
				};
			}
		}

		public class CompanyWrapper
		{
			public string Code { get; set; }
			public CodeNamePairWrapper Country { get; set; }
			public string Name { get; set; }

			internal static CompanyWrapper New(Company company)
			{
				return company == null ? null : new CompanyWrapper
				{
					Code = company.Code,
					Name = company.Name,
					Country = CodeNamePairWrapper.New(company.Country),
				};
			}
		}

		[XmlType("RecipientRole")]
		public class RecipientRoleWrapper
		{
			public static RecipientRoleWrapper New(ZString code, ZString description)
			{
				return new RecipientRoleWrapper { Code = code, Description = description };
			}

			[Mandatory]
			public string Code { get; set; }

			[MaxLength(50)]
			public ZString Description { get; set; }
		}

		public class CodeNamePairWrapper
		{
			public string Code { get; set; }
			public string Name { get; set; }

			internal static CodeNamePairWrapper New(ICodeNameDataObject value)
			{
				return value == null ? null : new CodeNamePairWrapper
				{
					Code = value.Code,
					Name = value.Name,
				};
			}
		}

		public class CodeDescriptionPairWrapper
		{
			public string Code { get; set; }
			public string Description { get; set; }

			internal static CodeDescriptionPairWrapper New(CodeDescriptionPair value)
			{
				return value == null ? null : new CodeDescriptionPairWrapper
				{
					Code = value.Code,
					Description = value.Description,
				};
			}
		}
	}
}
