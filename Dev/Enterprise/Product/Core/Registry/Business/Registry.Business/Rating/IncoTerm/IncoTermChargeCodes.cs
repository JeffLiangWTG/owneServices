using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Incoterms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class IncoTermChargeCodes : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string IncoTerm = "IncoTerm";
			public const string IncoTermDescription = "IncoTermDescription";
			public const string OriginBrokerage = "OriginBrokerage";
			public const string Origin = "Origin";
			public const string Loading = "Loading";
			public const string Freight = "Freight";
			public const string Insurance = "Insurance";
			public const string Unloading = "Unloading";
			public const string Destination = "Destination";
			public const string Brokerage = "Brokerage";
			public const string CustomsDuty = "CustomsDuty";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IncoTermChargeCodes();
		}

		#region Properties

		#region IncoTerm

		[ReadOnly(true)]
		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString IncoTerm
		{
			get { return fIncoTerm; }
			set
			{
				if (SetNonPersistentPropertyValue(IncoTermInfo, ref fIncoTerm, value) && !IsValidationSuspended)
				{
					ValidateIncoTerm();
				}
			}
		}

		ZString fIncoTerm;

		public ZPropertyInfo IncoTermInfo
		{
			get { return GetZPropertyInfo(Schema.IncoTerm); }
		}

		[CargoWise.ComponentModel.MaxLength(256)]
		public ZString IncoTermDescription
		{
			get { return IncoTermDescriptionMultilingual.GetUnresolvedString(); }
			set
			{
				CheckMaximumLength(IncoTermDescriptionInfo, value);
				IncoTermDescriptionMultilingual = (NoResString)value;
				ValidateIncoTermDescription();
				IncoTermDescriptionInfo.RefreshBinding();
			}
		}

		public ZString EnglishIncoTermDescription
		{
			get { return IncoTermDescriptionMultilingual.GetUnresolvedString(); }
			set
			{
				IncoTermDescription = (NoResString)value;
			}
		}

		public MultilingualString IncoTermDescriptionMultilingual
		{
			get
			{
				Incoterms.Descriptions.DefaultCodeDescriptionPairs.TryGetValue(IncoTerm, out var description);
				return incoTermDescriptionMultilingual ?? description ?? (NoResString)"";
			}
			set { incoTermDescriptionMultilingual = value; }
		}

		MultilingualString incoTermDescriptionMultilingual;

		public ZPropertyInfo IncoTermDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(IncoTermDescription)); }
		}

		public bool IncoTermDescription_ReadOnly => !IncotermsWithEditableDescriptionsList.Contains(IncoTerm);

		IIncotermValidation incoTermValidation;
		public IIncotermValidation IncotermValidation
			=> incoTermValidation ?? (incoTermValidation = ObjectFactory.Get<IIncotermValidation>());

		public void ValidateIncoTerm()
		{
			IncoTermInfo.ClearAllNotifications();
			IncotermValidation.WarningIfExpired(IncoTermInfo);
		}

		public void ValidateIncoTermDescription()
		{
			IncoTermDescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(IncoTermDescriptionInfo);
		}

		#endregion

		#region OriginBrokerage

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString OriginBrokerage
		{
			get { return fOriginBrokerage; }
			set
			{
				CheckMaximumLength(OriginBrokerageInfo, value);
				fOriginBrokerage = value;
				if (!IsValidationSuspended)
				{
					ValidateOriginBrokerage();
				}
				OriginBrokerageInfo.RefreshBinding();
			}
		}

		ZString fOriginBrokerage;

		public ZPropertyInfo OriginBrokerageInfo
		{
			get { return GetZPropertyInfo(Schema.OriginBrokerage); }
		}

		public void ValidateOriginBrokerage()
		{
			OriginBrokerageInfo.ClearAllNotifications();
			ValidateParty(OriginBrokerageInfo);
		}

		#endregion

		#region Origin

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Origin
		{
			get { return fOrigin; }
			set
			{
				CheckMaximumLength(OriginInfo, value);
				fOrigin = value;
				if (!IsValidationSuspended)
				{
					ValidateOrigin();
				}
				OriginInfo.RefreshBinding();
			}
		}

		ZString fOrigin;

		public ZPropertyInfo OriginInfo
		{
			get { return GetZPropertyInfo(IncoTermChargeCodes.Schema.Origin); }
		}

		public void ValidateOrigin()
		{
			OriginInfo.ClearAllNotifications();
			ValidateParty(OriginInfo);
		}

		#endregion

		#region Loading

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Loading
		{
			get { return fLoading; }
			set
			{
				CheckMaximumLength(LoadingInfo, value);
				fLoading = value;
				if (!IsValidationSuspended)
				{
					ValidateLoading();
				}
				LoadingInfo.RefreshBinding();
			}
		}

		ZString fLoading;

		public ZPropertyInfo LoadingInfo
		{
			get { return GetZPropertyInfo(IncoTermChargeCodes.Schema.Loading); }
		}

		public void ValidateLoading()
		{
			LoadingInfo.ClearAllNotifications();
			ValidateParty(LoadingInfo);
		}

		#endregion

		#region Freight

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Freight
		{
			get { return fFreight; }
			set
			{
				CheckMaximumLength(FreightInfo, value);
				fFreight = value;
				if (!IsValidationSuspended)
				{
					ValidateFreight();
				}
				FreightInfo.RefreshBinding();
			}
		}

		ZString fFreight;

		public ZPropertyInfo FreightInfo
		{
			get { return GetZPropertyInfo(IncoTermChargeCodes.Schema.Freight); }
		}

		public void ValidateFreight()
		{
			FreightInfo.ClearAllNotifications();
			ValidateParty(FreightInfo);
		}

		#endregion

		#region Insurance

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Insurance
		{
			get { return fInsurance; }
			set
			{
				CheckMaximumLength(InsuranceInfo, value);
				fInsurance = value;
				if (!IsValidationSuspended)
				{
					ValidateInsurance();
				}
				InsuranceInfo.RefreshBinding();
			}
		}

		ZString fInsurance;

		public ZPropertyInfo InsuranceInfo
		{
			get { return GetZPropertyInfo(IncoTermChargeCodes.Schema.Insurance); }
		}

		public void ValidateInsurance()
		{
			InsuranceInfo.ClearAllNotifications();
			ValidateParty(InsuranceInfo);
		}

		#endregion

		#region Unloading

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Unloading
		{
			get { return fUnloading; }
			set
			{
				CheckMaximumLength(UnloadingInfo, value);
				fUnloading = value;
				if (!IsValidationSuspended)
				{
					ValidateUnloading();
				}
				UnloadingInfo.RefreshBinding();
			}
		}

		ZString fUnloading;

		public ZPropertyInfo UnloadingInfo
		{
			get { return GetZPropertyInfo(IncoTermChargeCodes.Schema.Unloading); }
		}

		public void ValidateUnloading()
		{
			UnloadingInfo.ClearAllNotifications();
			ValidateParty(UnloadingInfo);
		}

		#endregion

		#region Destination

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Destination
		{
			get { return fDestination; }
			set
			{
				CheckMaximumLength(DestinationInfo, value);
				fDestination = value;
				if (!IsValidationSuspended)
				{
					ValidateDestination();
				}
				DestinationInfo.RefreshBinding();
			}
		}

		ZString fDestination;

		public ZPropertyInfo DestinationInfo
		{
			get { return GetZPropertyInfo(IncoTermChargeCodes.Schema.Destination); }
		}

		public void ValidateDestination()
		{
			DestinationInfo.ClearAllNotifications();
			ValidateParty(DestinationInfo);
		}

		#endregion

		#region Brokerage

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Brokerage
		{
			get { return fBrokerage; }
			set
			{
				CheckMaximumLength(BrokerageInfo, value);
				fBrokerage = value;
				if (!IsValidationSuspended)
				{
					ValidateBrokerage();
				}
				BrokerageInfo.RefreshBinding();
			}
		}

		ZString fBrokerage;

		public ZPropertyInfo BrokerageInfo
		{
			get { return GetZPropertyInfo(IncoTermChargeCodes.Schema.Brokerage); }
		}

		public void ValidateBrokerage()
		{
			BrokerageInfo.ClearAllNotifications();
			ValidateParty(BrokerageInfo);
		}

		#endregion

		#region CustomsDuty

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString CustomsDuty
		{
			get { return fCustomsDuty; }
			set
			{
				CheckMaximumLength(CustomsDutyInfo, value);
				fCustomsDuty = value;
				if (!IsValidationSuspended)
				{
					ValidateCustomsDuty();
				}
				CustomsDutyInfo.RefreshBinding();
			}
		}

		ZString fCustomsDuty;

		public ZPropertyInfo CustomsDutyInfo
		{
			get { return GetZPropertyInfo(IncoTermChargeCodes.Schema.CustomsDuty); }
		}

		public void ValidateCustomsDuty()
		{
			CustomsDutyInfo.ClearAllNotifications();
			ValidateParty(CustomsDutyInfo);
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateIncoTerm();
			ValidateIncoTermDescription();
			ValidateOriginBrokerage();
			ValidateOrigin();
			ValidateLoading();
			ValidateFreight();
			ValidateInsurance();
			ValidateUnloading();
			ValidateDestination();
			ValidateBrokerage();
			ValidateCustomsDuty();
		}

		void ValidateParty(ZPropertyInfo info)
		{
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info, Parties);
		}

		#endregion

		#region Lists

		public CodeDescriptionPairList Parties
		{
			get
			{
				if (parties == null)
				{
					parties = new CodeDescriptionPairList();
					parties.AddPair(Constants.PaymentParty.Consignor, ResString.GetMultilingualString("6af05a9d-88f2-4c5a-8fdd-73e2cc169563", "Consignor"));
					parties.AddPair(Constants.PaymentParty.Consignee, ResString.GetMultilingualString("560e3bc2-06fd-4690-a39c-dce017e2d3ff", "Consignee"));
				}

				return parties;
			}
		}

		CodeDescriptionPairList parties;

		internal ReadOnlyCollection<string> IncotermsWithEditableDescriptionsList = Array.AsReadOnly(new string[]
		{
			Incoterms.FreeCarrier,
			Incoterms.FreeCarrierBuyer,
			Incoterms.FreeCarrierSeller
		});

		#endregion

		#region Xml Serialisation

		readonly string[] serialisedProperties = new string[]
		{
			Schema.IncoTerm,
			Schema.IncoTermDescription,
			Schema.OriginBrokerage,
			Schema.Origin,
			Schema.Loading,
			Schema.Freight,
			Schema.Insurance,
			Schema.Unloading,
			Schema.Destination,
			Schema.Brokerage,
			Schema.CustomsDuty
		};

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			serialisedProperties.ForEach(prop => writer.WriteElementString(prop, this[prop].ToString()));
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IncoTerm = reader.ReadElementString(Schema.IncoTerm);
			SetDefaults(IncoTerm);

			var readerObject = reader.Reader;
			while (readerObject.NodeType != XmlNodeType.EndElement)
			{
				var (localName, element) = (readerObject.LocalName, readerObject.ReadElementString());
				if (serialisedProperties.Contains(localName))
				{
					this[localName] = element;
				}
			}
		}

		#endregion

		#region Default Values

		public void SetDefaults(string incoTerm)
		{
			using (GetValidationSuspender())
			{
				switch (incoTerm)
				{
					case Incoterms.ExWorks: // EXW - ExWorks
						SetDefaults_ExWorks();
						break;

					case Incoterms.FreeCarrier: // FCA - FreeCarrier
						SetDefaults_FreeCarrier();
						break;

					case Incoterms.FreeCarrierSeller:
						SetDefaults_FreeCarrierSeller();
						break;

					case Incoterms.FreeCarrierBuyer:
						SetDefaults_FreeCarrierBuyer();
						break;

					case Incoterms.FreeAlongsideShip: // FAS - FreeAlongsideShip
						SetDefaults_FreeAlongsideShip();
						break;

					case Incoterms.FreeOnBoard: // FOB - FreeOnBoard
						SetDefaults_FreeOnBoard();
						break;

					case Incoterms.CostAndFreight: // CFR - CostAndFreight
						SetDefaults_CostAndFreight();
						break;

					case Incoterms.CostInsuranceAndFreight: // CIF - CostInsuranceAndFreight
						SetDefaults_CostInsuranceAndFreight();
						break;

					case Incoterms.CarriagePaidTo: //  CPT - CarriagePaidTo
						SetDefaults_CarriagePaidTo();
						break;

					case Incoterms.CarriageAndInsurancePaidTo: //  CIP - CarriageAndInsurancePaidTo
						SetDefaults_CarriageAndInsurancePaidTo();
						break;

					case Incoterms.DeliveredAtFrontier: // DAF - DeliveredAtFrontier (obsolete in incoterms 2010)
						SetDefaults_DeliveredAtFrontier();
						break;

					case Incoterms.DeliveredExShip: // DES - DeliveredExShip (obsolete in incoterms 2010)
						SetDefaults_DeliveredExShip();
						break;

					case Incoterms.DeliveredExQuay: // DEQ - DeliveredExQuay (obsolete in incoterms 2010)
						SetDefaults_DeliveredExQuay();
						break;

					case Incoterms.DeliveredDutyUnpaid: // DDU - DeliveredDutyUnpaid (obsolete in incoterms 2010)
						SetDefaults_DeliveredDutyUnpaid();
						break;

					case Incoterms.DeliveredDutyPaid: // DDP - DeliveredDutyPaid
						SetDefaults_DeliveredDutyPaid();
						break;

					case Incoterms.DeliveredAtPlace: // DAP - DeliveredAtPlace
						SetDefaults_DeliveredAtPlace();
						break;

					case Incoterms.DeliveredAtTerminal: // DAT - DeliveredAtTerminal (obsolete in incoterms 2020)
						SetDefaults_DeliveredAtTerminal();
						break;

					case Incoterms.DeliveredAtPlaceUnloaded: // DPU - DeliveredAtPlaceUnloaded
						SetDefaults_DeliveredAtPlaceUnloaded();
						break;
				}
			}
		}

		#region Incoterm Defaults

		void SetDefaults_ExWorks()
		{
			IncoTerm = Incoterms.ExWorks;
			OriginBrokerage = Constants.PaymentParty.Consignee;
			Origin = Constants.PaymentParty.Consignee;
			Loading = Constants.PaymentParty.Consignee;
			Freight = Constants.PaymentParty.Consignee;
			Insurance = Constants.PaymentParty.Consignee;
			Unloading = Constants.PaymentParty.Consignee;
			Destination = Constants.PaymentParty.Consignee;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_FreeCarrier()
		{
			IncoTerm = Incoterms.FreeCarrier;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignee;
			Freight = Constants.PaymentParty.Consignee;
			Insurance = Constants.PaymentParty.Consignee;
			Unloading = Constants.PaymentParty.Consignee;
			Destination = Constants.PaymentParty.Consignee;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_FreeCarrierSeller()
		{
			IncoTerm = Incoterms.FreeCarrierSeller;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignee;
			Insurance = Constants.PaymentParty.Consignee;
			Unloading = Constants.PaymentParty.Consignee;
			Destination = Constants.PaymentParty.Consignee;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_FreeCarrierBuyer()
		{
			IncoTerm = Incoterms.FreeCarrierBuyer;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignee;
			Loading = Constants.PaymentParty.Consignee;
			Freight = Constants.PaymentParty.Consignee;
			Insurance = Constants.PaymentParty.Consignee;
			Unloading = Constants.PaymentParty.Consignee;
			Destination = Constants.PaymentParty.Consignee;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_FreeAlongsideShip()
		{
			IncoTerm = Incoterms.FreeAlongsideShip;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignee;
			Freight = Constants.PaymentParty.Consignee;
			Insurance = Constants.PaymentParty.Consignee;
			Unloading = Constants.PaymentParty.Consignee;
			Destination = Constants.PaymentParty.Consignee;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_FreeOnBoard()
		{
			IncoTerm = Incoterms.FreeOnBoard;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignee;
			Insurance = Constants.PaymentParty.Consignee;
			Unloading = Constants.PaymentParty.Consignee;
			Destination = Constants.PaymentParty.Consignee;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_CostAndFreight()
		{
			IncoTerm = Incoterms.CostAndFreight;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignee;
			Unloading = Constants.PaymentParty.Consignee;
			Destination = Constants.PaymentParty.Consignee;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_CostInsuranceAndFreight()
		{
			IncoTerm = Incoterms.CostInsuranceAndFreight;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignor;
			Unloading = Constants.PaymentParty.Consignee;
			Destination = Constants.PaymentParty.Consignee;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_CarriagePaidTo()
		{
			IncoTerm = Incoterms.CarriagePaidTo;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignee;
			Unloading = Constants.PaymentParty.Consignor;
			Destination = Constants.PaymentParty.Consignor;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_CarriageAndInsurancePaidTo()
		{
			IncoTerm = Incoterms.CarriageAndInsurancePaidTo;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignor;
			Unloading = Constants.PaymentParty.Consignor;
			Destination = Constants.PaymentParty.Consignor;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_DeliveredAtFrontier()
		{
			IncoTerm = Incoterms.DeliveredAtFrontier;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignor;
			Unloading = Constants.PaymentParty.Consignor;
			Destination = Constants.PaymentParty.Consignor;
			Brokerage = Constants.PaymentParty.Consignor;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_DeliveredExShip()
		{
			IncoTerm = Incoterms.DeliveredExShip;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignor;
			Unloading = Constants.PaymentParty.Consignee;
			Destination = Constants.PaymentParty.Consignee;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_DeliveredExQuay()
		{
			IncoTerm = Incoterms.DeliveredExQuay;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignor;
			Unloading = Constants.PaymentParty.Consignor;
			Destination = Constants.PaymentParty.Consignee;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_DeliveredDutyUnpaid()
		{
			IncoTerm = Incoterms.DeliveredDutyUnpaid;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignor;
			Unloading = Constants.PaymentParty.Consignor;
			Destination = Constants.PaymentParty.Consignor;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_DeliveredDutyPaid()
		{
			IncoTerm = Incoterms.DeliveredDutyPaid;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignor;
			Unloading = ZDateTime.UtcNow.ToDateTime() >= Incoterms.Incoterms2020EffectiveDate
				? Constants.PaymentParty.Consignee
				: Constants.PaymentParty.Consignor;
			Destination = Constants.PaymentParty.Consignor;
			Brokerage = Constants.PaymentParty.Consignor;
			CustomsDuty = Constants.PaymentParty.Consignor;
		}

		void SetDefaults_DeliveredAtPlace()
		{
			IncoTerm = Incoterms.DeliveredAtPlace;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignor;
			Unloading = ZDateTime.UtcNow.ToDateTime() >= Incoterms.Incoterms2020EffectiveDate
				? Constants.PaymentParty.Consignee
				: Constants.PaymentParty.Consignor;
			Destination = Constants.PaymentParty.Consignor;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_DeliveredAtTerminal()
		{
			IncoTerm = Incoterms.DeliveredAtTerminal;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignor;
			Unloading = Constants.PaymentParty.Consignor;
			Destination = Constants.PaymentParty.Consignor;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		void SetDefaults_DeliveredAtPlaceUnloaded()
		{
			IncoTerm = Incoterms.DeliveredAtPlaceUnloaded;
			OriginBrokerage = Constants.PaymentParty.Consignor;
			Origin = Constants.PaymentParty.Consignor;
			Loading = Constants.PaymentParty.Consignor;
			Freight = Constants.PaymentParty.Consignor;
			Insurance = Constants.PaymentParty.Consignor;
			Unloading = Constants.PaymentParty.Consignor;
			Destination = Constants.PaymentParty.Consignor;
			Brokerage = Constants.PaymentParty.Consignee;
			CustomsDuty = Constants.PaymentParty.Consignee;
		}

		#endregion

		#endregion
	}
}
