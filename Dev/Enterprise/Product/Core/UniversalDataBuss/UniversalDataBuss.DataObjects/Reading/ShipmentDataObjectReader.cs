using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public abstract class ShipmentDataObjectReader<TBusinessObject> : UniversalItemDataObjectReaderBase<UniversalShipment, TBusinessObject>, IShipmentDataObjectReader
		where TBusinessObject : BusinessObject
	{
		protected ShipmentDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		protected sealed override TBusinessObject GetExistingBusinessObject()
			=> TryGetExistingBusinessObject();

		public TBusinessObject TryGetExistingBusinessObject()
		{
			if (dataObject.DataContext != null)
			{
				BusinessObject foundBusinessObject = GetBusinessObjectFromContextKey();

				if (foundBusinessObject != null)
				{
					return (TBusinessObject)foundBusinessObject;
				}

				var shipmentDataContextmanager = DataContextManager as IShipmentDataContextManager;
				if (shipmentDataContextmanager != null)
				{
					foundBusinessObject = GetExistingBusinessObjectMatchedOnReferenceAndPartyIdIfEnabled();
					if (foundBusinessObject != null)
					{
						logger.LogVerboseOnly(LogType.Information, Res.GetString("547FC301-3479-4FA0-BE2F-00DA00E45C9D", "Found matching using reference and party id."));
						return (TBusinessObject)foundBusinessObject;
					}
				}
			}

			var result = GetExistingBusinessObjectUsingModuleSpecificBusinessRules();
			if (result != null)
			{
				logger.LogVerboseOnly(LogType.Information, Res.GetString("4011bab4-e99c-4a80-be0e-06c75797001c",
					"Found matching {0} using Module Specific Business Rules.", DataContextType.ToString()));

				return result;
			}

			if (!ReferenceAndPartyIDMatchingIsEnabled)
			{
				var matcher = GetCombinedReferenceMatcher();
				if (matcher != null)
				{
					var bestMatch = matcher.GetBestMatch();
					return bestMatch;
				}
			}

			return null;
		}

		protected bool ReferenceAndPartyIDMatchingIsEnabled
		{
			get
			{
				return (referenceAndPartyIDMatchingIsEnabled ?? (referenceAndPartyIDMatchingIsEnabled = ModuleHasReferenceAndPartyIDMatchingEnabled && ModuleHasReferenceAndPartyIDMatchingImplemented)).Value;
			}
		}
		bool? referenceAndPartyIDMatchingIsEnabled;

		BusinessObject GetExistingBusinessObjectMatchedOnReferenceAndPartyIdIfEnabled()
		{
			if (!ModuleHasReferenceAndPartyIDMatchingEnabled)
			{
				return null;
			}

			var matcher = GetReferenceAndPartyIDMatcher(factory);
			if (matcher == null)
			{
				return null;
			}

			var matchResult = matcher.GetBestMatch(this);

			matchResult.WriteLogs(logger);
			return matchResult.Success ? matchResult.MatchFound : null;
		}

		/// <summary>
		/// Can be overridden to provide information used for Reference and Party ID matching.
		/// This will become a mandatory requirement for all modules that can import a Universal Shipment.
		/// At that point it will be come abstract and ModuleHasReferenceAndPartyIDMatchingImplemented will be removed.
		/// </summary>
		/// <returns></returns>
		protected virtual IModuleMatcher<IShipmentDataObjectReader, TBusinessObject> GetReferenceAndPartyIDMatcher(UniversalObjectFactory factory)
		{
			return null;
		}

		public bool ModuleHasReferenceAndPartyIDMatchingImplemented
		{
			get
			{
				var method = GetType().GetMethod("GetReferenceAndPartyIDMatcher", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				return method != null && method.DeclaringType != typeof(ShipmentDataObjectReader<TBusinessObject>);
			}
		}

		protected virtual bool ModuleHasReferenceAndPartyIDMatchingEnabled
		{
			get { return eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.Value; }
		}

		public virtual IShipmentDataObjectReader ParentReader
		{
			get { return null; }
		}

		public ITopLevelDataObject DataObject
		{
			get { return dataObject; }
		}
	}
}
