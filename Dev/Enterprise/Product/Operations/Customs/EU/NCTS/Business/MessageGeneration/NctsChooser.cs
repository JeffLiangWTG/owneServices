using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public abstract class NctsChooser<T>
	{
		protected NctsChooser(ZString springTypeName, ZString nctsDomainCountryCode)
		{
			this.springTypeName = springTypeName;
			this.nctsDomainCountryCode = nctsDomainCountryCode;
			this.objectFromSpring = default(T);
			this.objectFromSpring = CountrySpecificChooser;
		}

#if DEBUG
		public
#else
		protected
#endif
		T CountrySpecificChooser
		{
			get
			{
				if (objectFromSpring == null)
				{
					var availableChoosers = DictionaryOfAvailableChoosers;
					if (availableChoosers.ContainsKey(nctsDomainCountryCode))
					{
						objectFromSpring = availableChoosers[nctsDomainCountryCode];
					}
				}
				return objectFromSpring;
			}
		}

		public ZString NctsDomainCountryCode
		{
			get { return nctsDomainCountryCode; }
		}

		public Dictionary<string, T> DictionaryOfAvailableChoosers
		{
			get => dictionaryOfAvailableChoosers ?? (dictionaryOfAvailableChoosers = GetListOfOfAvailableChoosersFromSpring(springTypeName));
		}

		protected static Dictionary<string, T> GetListOfOfAvailableChoosersFromSpring(ZString typeName)
		{
			var chooserList = new Dictionary<string, T>();
			if (!typeName.IsEmpty)
			{
				var dictionary = (Hashtable)ObjectFactory.Get(typeName);

				foreach (DictionaryEntry entry in dictionary)
				{
					var key = (string)entry.Key;
					var valueHandle = (ObjectHandle)entry.Value;
					var chooser = default(T);
					chooser = (T)valueHandle.GetObject();
					chooserList.Add(key, chooser);
				}
			}
			return chooserList;
		}

		protected static ZString GetNctsCountryFromNctsHeader(NctsHeader nctsHeader, NctsMessageFunctionSet messageFunction)
		{
			var nctsCountry = ZString.Empty;
			if (nctsHeader != null)
			{
				var departureCustomsOfficeCodeCountry = nctsHeader.IsPhase5 ? nctsHeader.CommonMovementHeader.DepartureCustomsOfficeCodeCountry : nctsHeader.DepartureCustomsOfficeCodeCountry;
				if (messageFunction is NctsMessageFunctionSet.DeclarationDataMessage || messageFunction is NctsMessageFunctionSet.DeclarationCancellationRequestMessage
					|| messageFunction is NctsMessageFunctionSet.InformationAboutNonArrivedMovementMessage || messageFunction is NctsMessageFunctionSet.DeclarationAmendmentMessage)
				{
					nctsCountry = departureCustomsOfficeCodeCountry == Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes ? Core.Constants.CountryCodes.UnitedKingdom : departureCustomsOfficeCodeCountry.ToString();
				}
				else if (messageFunction is NctsMessageFunctionSet.ArrivalNotificationMessage || messageFunction is NctsMessageFunctionSet.UnloadingRemarksMessage || messageFunction is NctsMessageFunctionSet.CombinedArrivalAndDepartureMessage)
				{
					nctsCountry = nctsHeader.IsPhase5 ? nctsHeader.ArrivalMovementHeader.DestinationCustomsOfficeCodeCountryForArrival.ToString() : nctsHeader.DestinationCustomsOfficeCodeCountryForArrival.ToString();
				}
			}
			return nctsCountry;
		}

		protected static ZString GetNctsCountryFromMessages(NonDependentEDIMessageCollection messages)
		{
			if (messages == null || messages.Count == 0)
			{
				return ZString.Empty;
			}
			else
			{
				return messages[0].EM_MessageType;
			}
		}

		readonly ZString springTypeName;
		readonly ZString nctsDomainCountryCode;
		Dictionary<string, T> dictionaryOfAvailableChoosers;
		T objectFromSpring;
	}
}
