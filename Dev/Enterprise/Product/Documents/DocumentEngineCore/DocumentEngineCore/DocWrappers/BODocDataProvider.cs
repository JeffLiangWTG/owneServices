using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Application.Exceptions;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class BODocDataProvider : IBODocDataProvider
	{
		protected BODocDataProvider(BusinessObject parent)
		{
			this.parent = parent;
		}

		public static IBODocDataProvider Get(BusinessObject parent)
		{
			return parent as IBODocDataProvider ?? GetDefault(parent);
		}

		public static IBODocDataProvider GetDefault(BusinessObject parent)
		{
			return parent == null ? null : new BODocDataProvider(parent);
		}

		public static BusinessObject GetBusinessObject(object docDataProvider)
		{
			return (BusinessObject)GetObject(docDataProvider);
		}

		public static object GetObject(object docDataProvider)
		{
			BODocDataProvider dataProvider = docDataProvider as BODocDataProvider;
			return dataProvider != null ? dataProvider.parent : docDataProvider;
		}

		public static bool IsBODocDataProvider(Type type)
		{
			if (typeof(IBODocDataProvider).IsAssignableFrom(type) || typeof(BusinessObject).IsAssignableFrom(type))
			{
				return true;
			}

			if (type.IsInterface)
			{
				try
				{
					var concreteType = ObjectFactory.GetType(type);
					return typeof(IBODocDataProvider).IsAssignableFrom(concreteType) || typeof(BusinessObject).IsAssignableFrom(concreteType);
				}
				catch (NoSuchObjectDefinitionException) { }
			}

			return false;
		}

		public static ZString GetDocDataValue(BusinessObject parent, ZString docDataIdentifier, ZString formatStringForFallbackValue)
		{
			var iBODocDataProvider = BODocDataProvider.Get(parent);
			return iBODocDataProvider == null ? ZString.Empty : iBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);
		}

		public static IBODocDataProvider[] GetArray(BusinessObject[] parents)
		{
			IBODocDataProvider[] result = new IBODocDataProvider[parents.Length];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = BODocDataProvider.Get(parents[i]);
			}
			return result;
		}

		public ZString DocTypeCode { get; set; }

		public BusinessObject BusinessObjectToLogAgainst
		{
			get { return parent; }
		}

		public string[] ImageNamesToRemove
		{
			get { return Array.Empty<string>(); }
		}

		public BusinessObject ParentBusinessObject
		{
			get { return parent; }
		}

		public override string ToString()
		{
			return parent.ToString();
		}

		public ZString GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue)
		{
			ZString result = DocWrapperDataManager.GetValue(docDataIdentifier);

			if (result.IsEmpty && !formatStringForFallbackValue.IsEmpty)
			{
				var intrepreter = ObjectFactory.Get<IFormatStringInterpreter>();
				result = intrepreter.Format(parent, formatStringForFallbackValue);
			}

			return result;
		}

		public IDocDataManager DocWrapperDataManager
		{
			get { return docDataManager ?? (docDataManager = (IDocDataManager)Activator.CreateInstance(ObjectFactory.GetType<IDocDataManager>(), new object[] { parent })); }
		}
		IDocDataManager docDataManager;

		public DocWrapperCopyInfo AdditionalCopyInfo
		{
			get { return null; }
		}

		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants)
		{
			if (ParentBusinessObject.Factory != null)
			{
				ParentBusinessObject.Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(constants);
			}
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			BODocDataProvider rhs = obj as BODocDataProvider;
			return rhs != null && parent == rhs.parent;
		}

		public override int GetHashCode()
		{
			return parent.GetHashCode();
		}

		#endregion

		readonly BusinessObject parent;

		#region GetCustomField

		public IZType GetCustomField(string fieldName, string typeName)
		{
			return GetCustomField(PreprocessCustomFieldPart(fieldName), typeName, (IGetCustomFieldStrategy)Activator.CreateInstance(ObjectFactory.GetType<IGetCustomFieldStrategy>(), new object[] { parent }));
		}

		internal IZType GetCustomField(string fieldName, string typeName, IGetCustomFieldStrategy strategy)
		{
			return strategy.GetCustomField(PreprocessCustomFieldPart(fieldName), typeName);
		}

		public static IZType GetCustomField(BusinessObject parent, string fieldName, string typeName)
		{
			var iBODocDataProvider = BODocDataProvider.Get(parent);
			return iBODocDataProvider == null ? ZString.Empty : iBODocDataProvider.GetCustomField(PreprocessCustomFieldPart(fieldName), typeName);
		}

		public const string PartIdentifier = "PART";

		static string PreprocessCustomFieldPart(string input)
		{
			var result = input;
			var regex = new Regex(@",\s*(\d+)$");
			if (regex.IsMatch(input))
			{
				result = regex.Replace(input, (match => { return PartIdentifier + match.Groups[1]; }));
			}
			return result;
		}

		#endregion

		#region GetCustomFieldCodeDescription

		public string GetCustomFieldCodeDescription(string fieldName, string typeName)
		{
			return GetCustomFieldCodeDescription(fieldName, typeName, (IGetCustomFieldStrategy)Activator.CreateInstance(ObjectFactory.GetType<IGetCustomFieldStrategy>(), new object[] { parent }));
		}

		internal string GetCustomFieldCodeDescription(string fieldName, string typeName, IGetCustomFieldStrategy strategy)
		{
			return strategy.GetCustomFieldCodeDescription(fieldName, typeName);
		}

		public static ZString GetCustomFieldCodeDescription(BusinessObject parent, string fieldName, string typeName)
		{
			var iBODocDataProvider = BODocDataProvider.Get(parent);
			return iBODocDataProvider == null ? String.Empty : iBODocDataProvider.GetCustomFieldCodeDescription(fieldName, typeName);
		}

		#endregion

		#region GetEventLastDatetime

		public ZDateTime GetEventLastDateTime(string eventCode)
		{
			Argument.NotNull(eventCode, "eventCode");

			IStmALogParent logParent = parent as IStmALogParent;
			if (logParent != null)
			{
				if (Events.All[eventCode] == null)
				{
					var errorMessage = string.IsNullOrEmpty(eventCode) ? Res.GetString("7BF8BF84-1E77-4566-9441-CB7DE46C76E0", "No Event Code was entered.") :
						Res.GetString("4E066C75-89F6-41E9-8FF2-FFFCE8749830", "Invalid Event Code ({0}) was entered.", eventCode);
					throw new InvalidDocumentWrapperParameterException(errorMessage);
				}

				var query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
				query.AddToFilter(StmALogSchema.SL_IsEstimate, false);
				StmALog log = logParent.Logs.MostRecentLogByEventTime(Events.All[eventCode], query);
				if (log != null)
				{
					return log.SL_EventTime;
				}
			}
			return ZDateTime.Empty;
		}

		public static ZDateTime GetEventLastDateTime(BusinessObject parent, string eventCode)
		{
			return BODocDataProvider.Get(parent).GetEventLastDateTime(eventCode);
		}

		#endregion
	}
}
