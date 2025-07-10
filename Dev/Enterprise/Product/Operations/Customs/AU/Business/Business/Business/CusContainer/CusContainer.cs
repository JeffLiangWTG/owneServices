using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusContainer : BaseCusContainer, IAggregatedAddInfo, IAddInfoManager, ISupportDataImporting, IPRAContainerMessaging, Integration.Customs.AU.ICusContainer
	{
		#region Schema

		public new abstract class Schema : BaseCusContainer.Schema
		{
			public const string CurrentPRAStatus = "CurrentPRAStatus";
			public const string SealStartNumber = "SealStartNumber";
			public const string SealEndNumber = "SealEndNumber";
		}

		#endregion

		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CusContainer New(BusinessObjectFactory factory)
		{
			return (CusContainer)factory.New(typeof(CusContainer));
		}

		#region ISupportDataImporting

		bool fIsImportingData;
		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#endregion

		#region Related Business Objects

		public JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)Factory.Load(typeof(JobDeclaration), CO_JE); }
		}

		public RefContainerCollection ContainerTypeCollection
		{
			get
			{
				if (fContainerTypeCollection == null)
				{
					fContainerTypeCollection = new RefContainerCollection(Factory);
				}
				return fContainerTypeCollection;
			}
		}
		RefContainerCollection fContainerTypeCollection;

		#endregion

		#region Lists

		public CodeDescriptionPairList CO_FCL_LCL_NCT_List
		{
			get
			{
				return Factory.GetCachedValue("AUCus_CO_FCL_LCL_NCT_List", () => new AUCusContainerModeList());
			}
		}

		#endregion

		#region Properties

		public override ZString CO_ContainerNumber
		{
			get { return base.CO_ContainerNumber; }
			set { base.CO_ContainerNumber = value.ToUpper(); }
		}

		public override ZString CO_Seal
		{
			get { return base.CO_Seal; }
			set { base.CO_Seal = value.ToUpper(); }
		}

		public ZString CO_FCL_LCL_AIR_ForMessaging
		{
			get
			{
				if (CO_FCL_LCL_AIR == Core.Constants.ContainerModes.BreakBulk)
				{
					return CMRCargoTypes.Codes.BreakBulk;
				}
				else
				{
					return CO_FCL_LCL_AIR;
				}
			}
		}

		#region IPRAContainerMessaging Members

		public PRAMessageCollection PRAMessages
		{
			get
			{
				if (praMessages == null)
				{
					praMessages = new PRAMessageCollection(this, Factory);
					praMessages.Load();
					praMessages.IsManagedForDataRefresh = true;
				}

				return praMessages;
			}
		}
		PRAMessageCollection praMessages;

		public bool LastPRAMessageSentWasCancellation => Factory.GetValue(ref lastPRAMessageSentWasCancellation, this.GetLastPRAMessageSentWasCancellation);

		CachedProperty<bool> lastPRAMessageSentWasCancellation;

		public ZString CurrentPRAStatus => Factory.GetValue(ref currentPRAStatus, this.GetCurrentPRAStatus);

		CachedProperty<ZString> currentPRAStatus;

		public ZPropertyInfo CurrentPRAStatusInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentPRAStatus); }
		}

		#endregion

		#region ExDocsSealNumbers

		public ZString SealStartNumber
		{
			get { return AddInfo.ZA_AQISSealStart_Hidden; }
			set
			{
				if (AddInfo.ZA_AQISSealStart_Hidden != value)
				{
					AddInfo.ZA_AQISSealStart_Hidden = value;
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo SealStartNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SealStartNumber, x => AddInfo.ZA_AQISSealStart_HiddenInfo); }
		}

		public ZString SealEndNumber
		{
			get { return AddInfo.ZA_AQISSealEnd_Hidden; }
			set
			{
				if (AddInfo.ZA_AQISSealEnd_Hidden != value)
				{
					AddInfo.ZA_AQISSealEnd_Hidden = value;
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo SealEndNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SealEndNumber, x => AddInfo.ZA_AQISSealEnd_HiddenInfo); }
		}

		#endregion

		#region AddInfo

		public AUAddInfo AddInfo
		{
			get
			{
				if (addInfo == null)
				{
					addInfo = new CusContainerAddInfo(this, CO_AddInfoInfo);
					RegisterEditableChildObject(addInfo);
				}
				return addInfo;
			}
		}
		protected AUAddInfo addInfo;

		[BusinessObjectTestExclude()]
		public override ZString CO_AddInfo
		{
			get { return base.CO_AddInfo; }
			set
			{
				value = value.Trim(AUAddInfo.SeperationCharacter);
				if (CO_AddInfo != value)
				{
					base.CO_AddInfo = value;
					using (AddInfo.GetValidationSuspender())
					{
						AddInfo.LoadPropertiesFromString(CO_AddInfo);
					}
				}
			}
		}

		bool IAggregatedAddInfo.IsCopying
		{
			get { return IsCopying; }
		}

		public IZType AggregatedValue(string propertyName)
		{
			return (IZType)AddInfo[propertyName];
		}

		ZString IAggregatedAddInfo.AggregatedZA_ORG
		{
			get { return ZString.Empty; }
		}

		ZString IAggregatedAddInfo.AggregatedZA_PRF
		{
			get { return ZString.Empty; }
		}

		public ZDateTime EffectiveDutyDate
		{
			get { return ZDateTime.Today; }
		}

		public ZDateTime DateOfValuation
		{
			get { return ZDateTime.Today; }
		}

		#endregion

		#endregion

		#region Implementation

		protected internal Customs.Business.CusContainerValidation GetNewValidationInternal() => GetNewValidation();
		protected override Customs.Business.CusContainerValidation GetNewValidation()
		{
			if (JobDeclaration != null && JobDeclaration.IsImportCMR)
			{
				return new IMDCusContainerValidation(this);
			}
			else
			{
				return new EdificeCusContainerValidation(this);
			}
		}

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && IsDeclarationPersistent; }
		}

		public bool IsDeclarationPersistent
		{
			get
			{
				var declaration = Declaration;
				return declaration == null || declaration.IsDeleted || declaration.IsPersistent;
			}
		}

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion
	}
}
