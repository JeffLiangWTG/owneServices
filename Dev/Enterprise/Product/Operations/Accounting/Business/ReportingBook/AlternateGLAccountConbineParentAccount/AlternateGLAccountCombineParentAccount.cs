using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	[CodeProperty(nameof(AlternateAccountNum))]
	[DescriptionProperty(nameof(AlternateAccountName))]
	public class AlternateGLAccountCombineParentAccount : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AlternateGLAccountCombineParentAccount(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public static class Schema
		{
			public const string Attribute_ORG = "Attribute_ORG";
			public const string Attribute_OCG = "Attribute_OCG";
			public const string Attribute_LFE = "Attribute_LFE";
			public const string Attribute_LFO = "Attribute_LFO";
			public const string Attribute_TIC = "Attribute_TIC";
			public const string Attribute_SPR = "Attribute_SPR";
			public const string AlternateAccountName = "AlternateAccountName";
			public const string AlternateAccountNum = "AlternateAccountNum";
		}

		#endregion

		#region Properties

		AccAlternateGLAccount alternateGLAccount;

		public AccAlternateGLAccount AlternateGLAccount
		{
			get
			{
				if (alternateGLAccount == null)
				{
					alternateGLAccount = Factory.New<AccAlternateGLAccount>();
				}
				return alternateGLAccount;
			}
			set
			{
				alternateGLAccount = value;
			}
		}

		#region AlternateAccountName

		public ZString AlternateAccountName => AlternateGLAccount?.AGA_Description ?? ZString.Empty;

		public ZPropertyInfo AlternateAccountNameInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AlternateAccountName));
			}
		}

		#endregion

		#region AlternateAccountNum

		public ZString AlternateAccountNum => AlternateGLAccount?.AGA_AccountNum ?? ZString.Empty;

		public ZPropertyInfo AlternateAccountNumInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AlternateAccountNum));
			}
		}

		#endregion

		#region GLHeaderPK

		ZGuid gLHeaderPK;
		public ZGuid GLHeaderPK
		{
			get { return gLHeaderPK; }
			set { gLHeaderPK = value; }
		}

		public ZPropertyInfo AAA_AG_GLHeaderInfo
		{
			get { return GetZPropertyInfo(nameof(GLHeaderPK)); }
		}

		#endregion

		#region Chart Code

		public ZString ChartCode => AlternateGLAccount.AGA_AAC_AlternateChart == ZGuid.Empty ? ZString.Empty : AlternateGLAccount.AlternateChart.AAC_Code;

		public ZPropertyInfo ChartCodeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ChartCode));
			}
		}

		#endregion

		#region Chart Name

		public ZString ChartName => AlternateGLAccount.AGA_AAC_AlternateChart == ZGuid.Empty ? ZString.Empty : AlternateGLAccount.AlternateChart.AAC_Description;

		public ZPropertyInfo ChartNameInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ChartName));
			}
		}

		#endregion

		#region Is Global

		public ZBool IsGlobal => AlternateGLAccount.AGA_AAC_AlternateChart == ZGuid.Empty ? ZBool.False : AlternateGLAccount.AlternateChart.AAC_IsGlobal;

		public ZPropertyInfo IsGlobalInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(IsGlobal));
			}
		}

		#endregion

		#region Parent Account

		public AccGLHeader GlHeader => Factory.Load<AccGLHeader>(GLHeaderPK);

		public ZString ParentAccount => GlHeader?.AG_AccountNum ?? ZString.Empty;

		public ZString ParentAccountName => GlHeader?.AG_DescriptionMultilingual ?? ZString.Empty;

		public ZPropertyInfo ParentAccountInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ParentAccount));
			}
		}

		#endregion

		#region AlternateNum

		public ZString AlternateNum => AlternateGLAccount.AGA_AGA_AlternateNum == ZGuid.Empty ? ZString.Empty : AlternateGLAccount.AlternateNum.AGA_AccountNum;

		public ZPropertyInfo AlternateNumInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AlternateNum));
			}
		}

		#endregion

		#region PercentNum

		public ZString PercentNum => AlternateGLAccount.AGA_AGA_PercentNum == ZGuid.Empty ? ZString.Empty : AlternateGLAccount.PercentNum.AGA_AccountNum;

		public ZPropertyInfo PercentNumInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(PercentNum));
			}
		}

		#endregion

		#region ConsolidationNum

		public ZString ConsolidationNum => AlternateGLAccount.AGA_AGA_ConsolidationNum == ZGuid.Empty ? ZString.Empty : AlternateGLAccount.ConsolidationNum.AGA_AccountNum;

		public ZPropertyInfo ConsolidationNumInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ConsolidationNum));
			}
		}

		#endregion

		#region HeaderDependsOnTotal

		public ZString HeaderDependsOnTotal => AlternateGLAccount.AGA_AGA_HeaderDependsOnTotal == ZGuid.Empty ? ZString.Empty : AlternateGLAccount.HeaderDependsOnTotal.AGA_AccountNum;

		public ZPropertyInfo HeaderDependsOnTotalInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(HeaderDependsOnTotal));
			}
		}

		#endregion

		#region AlternateGLAccountAttributes

		public AccAlternateGLAccountAttributeCollection AlternateGLAccountAttributes
		{
			get
			{
				if (alternateGLAccountAttributes == null)
				{
					alternateGLAccountAttributes = new AccAlternateGLAccountAttributeCollection(Factory);
					var query = new ZQuery(AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount, AlternateGLAccount.PK);
					query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, GLHeaderPK);
					alternateGLAccountAttributes.Load(query);
					RegisterEditableChildObject(alternateGLAccountAttributes);
				}
				return alternateGLAccountAttributes;
			}
		}
		AccAlternateGLAccountAttributeCollection alternateGLAccountAttributes;

		#endregion

		#region Audit Information

		public ZDateTime CreateTime => AlternateGLAccount.AGA_SystemCreateTimeUtc.ToLocalBranchTime();

		public ZPropertyInfo CreateTimeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(CreateTime));
			}
		}

		public ZDateTime LastEditTime => AlternateGLAccount.AGA_SystemLastEditTimeUtc.ToLocalBranchTime();

		public ZPropertyInfo LastEditTimeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(LastEditTime));
			}
		}

		#endregion

		#region Attributes

		public ZString oRGAttribute;

		public ZString ORGAttribute
		{
			get { return oRGAttribute; }
			set
			{
				SetNonPersistentPropertyValue(ORGAttributeInfo, ref oRGAttribute, value);
			}
		}

		public ZPropertyInfo ORGAttributeInfo => GetZPropertyInfo(nameof(ORGAttribute));

		public ZString OCGAttribute => oCGAttribute.IsEmpty ? ZString.Empty : oCGAttribute + " - " + AccountingMasterFilesConstants.OCGList.GetDescriptionFromCode(oCGAttribute);

		public ZPropertyInfo OCGAttributeInfo => GetZPropertyInfo(nameof(OCGAttribute));

		public ZString oCGAttribute;

		public ZString OCG_Attribute
		{
			get { return oCGAttribute; }
			set { SetNonPersistentPropertyValue(OCG_AttributeInfo, ref oCGAttribute, value); }
		}

		public ZPropertyInfo OCG_AttributeInfo => GetZPropertyInfo(nameof(OCG_Attribute));

		public ZString TICAttribute => tICAttribute.IsEmpty ? ZString.Empty : tICAttribute + " - " + AccountingMasterFilesConstants.TICList.GetDescriptionFromCode(tICAttribute);

		public ZPropertyInfo TICAttributeInfo => GetZPropertyInfo(nameof(TICAttribute));

		public ZString tICAttribute;

		public ZString TIC_Attribute
		{
			get { return tICAttribute; }
			set { SetNonPersistentPropertyValue(TIC_AttributeInfo, ref tICAttribute, value); }
		}

		public ZPropertyInfo TIC_AttributeInfo => GetZPropertyInfo(nameof(TIC_Attribute));

		public ZString SPRAttribute => sPRAttribute.IsEmpty ? ZString.Empty : sPRAttribute + " - " + AccountingMasterFilesConstants.SPRList.GetDescriptionFromCode(sPRAttribute);

		public ZPropertyInfo SPRAttributeInfo => GetZPropertyInfo(nameof(SPRAttribute));

		public ZString sPRAttribute;

		public ZString SPR_Attribute
		{
			get { return sPRAttribute; }
			set { SetNonPersistentPropertyValue(SPR_AttributeInfo, ref sPRAttribute, value); }
		}

		public ZPropertyInfo SPR_AttributeInfo => GetZPropertyInfo(nameof(SPR_Attribute));

		public ZString LFOAttribute => lFOAttribute.IsEmpty ? ZString.Empty : lFOAttribute + " - " + AccountingMasterFilesConstants.LFOList.GetDescriptionFromCode(lFOAttribute);

		public ZPropertyInfo LFOAttributeInfo => GetZPropertyInfo(nameof(LFOAttribute));

		public ZString lFOAttribute;

		public ZString LFO_Attribute
		{
			get { return lFOAttribute; }
			set { SetNonPersistentPropertyValue(LFO_AttributeInfo, ref lFOAttribute, value); }
		}

		public ZPropertyInfo LFO_AttributeInfo => GetZPropertyInfo(nameof(LFO_Attribute));

		public ZString LFEAttribute => lFEAttribute.IsEmpty ? ZString.Empty : lFEAttribute + " - " + AccountingMasterFilesConstants.LFEList.GetDescriptionFromCode(lFEAttribute);

		public ZPropertyInfo LFEAttributeInfo => GetZPropertyInfo(nameof(LFEAttribute));

		public ZString lFEAttribute;

		public ZString LFE_Attribute
		{
			get { return lFEAttribute; }
			set { SetNonPersistentPropertyValue(LFE_AttributeInfo, ref lFEAttribute, value); }
		}

		public ZPropertyInfo LFE_AttributeInfo => GetZPropertyInfo(nameof(LFE_Attribute));

		#endregion

		#endregion
	}
}
