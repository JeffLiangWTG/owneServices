using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	/// <summary>
	/// Wraps an object and exposes the properties for the Document Engine.
	/// </summary>
	public abstract class DocumentWrapper : NonPersistentBusinessObject, IObsoleteValidation, IBODocDataProvider, IDocumentWrapper, IBODocDataProviderWithBOForPrintJob
	{
		protected DocumentWrapper()
		{
		}

		protected DocumentWrapper(object objectToWrap, BusinessObjectFactory factory)
			: base(factory)
		{
			if (objectToWrap != null)
			{
				WrappedObject = objectToWrap;
			}
		}

		protected override void AddToFactoryCache()
		{
			// DocumentWrapper must not be cached in a Factory. It cause memory leaks. 
		}

		#region Attributes for tests
		public sealed class AllowPublicConstructor : Attribute
		{
		}
		public sealed class AllowNoStaticNew : Attribute
		{
		}
		#endregion

		public readonly Object WrappedObject;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected const string NoDefaultPropertyErrorMessage = "ERROR: There is no default property of this item";

		public override string ToString()
		{
			return DefaultFieldAttribute.GetDefaultValue(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return WrapperTypeNameAttribute.GetName(this.GetType()); }
		}

		protected virtual ZString GetDocDataValueOnly(ZString docDataIdentifier)
		{
			return ZString.Empty;
		}

		public ZDateTime Now
		{
			get { return ZDateTime.Now; }
		}

		public ZDateTimeOffset NowOffset => ZDateTimeOffset.Now;

		public virtual BusinessObject DeliveryContact
		{
			get { return null; }
		}

		#region Freight Labels Properties

		public ZBool DocIncludeConsignee { get; set; }
		public ZBool DocIncludeConsignor { get; set; }
		public ZBool DocIncludeNone { get; set; }
		public ZInt DocNumberOfLabelsToPrint { get; set; }
		public ZInt DocNumber { get; set; }

		#endregion

		#region Import Cargo Label Properties

		public ZBool DocIncludeHouseBill { get; set; }
		public ZBool DocIncludeCFSName { get; set; }

		#endregion

		#region IBODocDataProvider Members

		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst
		{
			get { return BusinessObjectToLogAgainst; }
		}

		protected virtual BusinessObject BusinessObjectToLogAgainst
		{
			get { return WrappedObject as BusinessObject; }
		}

		BusinessObject IBODocDataProvider.ParentBusinessObject
		{
			get { return ParentBusinessObject; }
		}

		protected virtual BusinessObject ParentBusinessObject
		{
			get { return WrappedObject as BusinessObject; }
		}

		protected virtual BusinessObject BusinessObjectForCustomFields
		{
			get { return ParentBusinessObject; }
		}

		public virtual ZString GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue)
		{
			ZString result = GetDocDataValueOnly(docDataIdentifier);
			if (result.IsEmpty && !formatStringForFallbackValue.IsEmpty)
			{
				using (var intrepreter = ObjectFactory.Get<IFormatStringInterpreter>())
				{
					result = intrepreter.Format((IBODocDataProvider)this, formatStringForFallbackValue);
				}
			}
			return result;
		}

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo
		{
			get { return AdditionalCopyInfo; }
		}

		protected virtual DocWrapperCopyInfo AdditionalCopyInfo
		{
			get { return additionalCopyInfo; }
		}
		DocWrapperCopyInfo additionalCopyInfo;

		public void SetAdditionalCopyInfo(DocWrapperCopyInfo additionalCopyInfo)
		{
			this.additionalCopyInfo = additionalCopyInfo;
			SetAdditionalCopyInfoCore(additionalCopyInfo);
		}

		protected virtual void SetAdditionalCopyInfoCore(DocWrapperCopyInfo additionalCopyInfo)
		{
		}

		string[] IBODocDataProvider.ImageNamesToRemove
		{
			get { return ImageNamesToRemove; }
		}

		protected virtual string[] ImageNamesToRemove
		{
			get { return BODocDataProvider.GetDefault(this).ImageNamesToRemove; }
		}

		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants)
		{
			SetDocWrapperContext(constants);
		}

		protected virtual void SetDocWrapperContext(Dictionary<string, object> constants)
		{
			BODocDataProvider.GetDefault(this).SetDocWrapperContext(constants);
		}

		string IBODocDataProvider.ToString()
		{
			return BODocDataProvider.GetDefault(this).ToString();
		}

		public IZType GetCustomField(string fieldName, string typeName = null)
		{
			if (BusinessObjectForCustomFields != null)
			{
				return BODocDataProvider.GetCustomField(BusinessObjectForCustomFields, fieldName, typeName);
			}

			return ZString.Empty;
		}

		public string GetCustomFieldCodeDescription(string fieldName, string typeName = null)
		{
			var result = string.Empty;

			if (BusinessObjectForCustomFields != null)
			{
				result = BODocDataProvider.GetCustomFieldCodeDescription(BusinessObjectForCustomFields, fieldName, typeName);
			}

			return result;
		}

		public ZDateTime GetEventLastDateTime(string eventCode)
		{
			var result = ZDateTime.Empty;
			if (ParentBusinessObject != null)
			{
				result = BODocDataProvider.GetEventLastDateTime(ParentBusinessObject, eventCode);
			}
			return result;
		}

		#endregion

		#region IBoDocDataProviderWithBoForPrintJob

		public virtual BusinessObject BusinessObjectForPrintJob => BusinessObjectToLogAgainst;

		#endregion
	}
}
