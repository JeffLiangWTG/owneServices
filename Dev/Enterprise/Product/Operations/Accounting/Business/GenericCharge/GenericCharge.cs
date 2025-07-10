using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GenericCharge
{
	/// <summary>
	/// Summary description for GenericCharge.
	/// </summary>
	[TestedAsNonPersistentBusinessObject]
	[CodeProperty(AutoViewGenericCharge.Schema.VC_Code)]
	[DescriptionProperty(AutoViewGenericCharge.Schema.VC_Description)]
	[RestrictedFilteredItem]
	public class GenericCharge : AutoViewGenericCharge
	{
		public GenericCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		public override void Delete()
		{
			// base.Delete ();
			ErrorReporter.ReportOnce(typeof(GenericCharge).FullName + "cannot delete", "Cannot delete accounting object " + nameof(GenericCharge));
		}

		public override bool CanDelete
		{
			get { return false; }
		}

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal VC_Percentage
		{
			get => base.VC_Percentage;
			set => base.VC_Percentage = value;
		}

		#endregion

		#region Lists

		CodeDescriptionPairList fAccountChargeTypeList;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public CodeDescriptionPairList AccountChargeTypeList
		{
			get
			{
				if (fAccountChargeTypeList == null)
				{
					fAccountChargeTypeList = new CodeDescriptionPairList();
					fAccountChargeTypeList.Insert(0, new CodeDescriptionPair("GL Account", Res.GetString("c978f394-fe8f-4296-8b61-64e78daea8bc", "GL Accounts Only")));
					fAccountChargeTypeList.Insert(1, new CodeDescriptionPair("Charge Code", Res.GetString("5458b62b-dcc5-4764-9ae8-91f02ead1509", "Charge Codes Only")));
				}
				return fAccountChargeTypeList;
			}
		}

		CodeDescriptionPairList fChargeTypeList;
		public CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				if (fChargeTypeList == null)
				{
					fChargeTypeList = new CodeDescriptionPairList(OLookUpEditType.ChargeTypes);
					fChargeTypeList.AddRange(new CodeDescriptionPairList(OLookUpEditType.GLAccountType));
				}
				return fChargeTypeList;
			}
		}

		#endregion

		public int PercentageDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages;

		public bool IsMargin
		{
			get { return VC_Type == Core.Constants.ChargeType.Margin; }
		}

		public bool IsDisbursement
		{
			get { return VC_Type == Core.Constants.ChargeType.Disbursement; }
		}

		public bool IsNonAccrual
		{
			get { return VC_Type == Core.Constants.ChargeType.NonAccrual; }
		}

		public bool IsRevenue
		{
			get { return VC_Type == Core.Constants.ChargeType.Revenue; }
		}

		public bool IsComment
		{
			get { return VC_Type == Core.Constants.ChargeType.Comment; }
		}

		public bool IsManualJobAccrual
		{
			get { return VC_Type == Core.Constants.ChargeType.ManualJobAccrual; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded sql words")]
		AccGLAccountDescriptor LocalAccountDescriptor
		{
			get
			{
				if (VC_TableName != "Charge Code" && !hasLoadedLocalDescriptor)
				{
					localAccountDescriptor = AccGLAccountDescriptor.GetLocalAccountDescriptor(Factory, PK);
					hasLoadedLocalDescriptor = true;
				}

				return localAccountDescriptor;
			}
		}
		AccGLAccountDescriptor localAccountDescriptor;

		bool hasLoadedLocalDescriptor;

		public ZString LocalAccountNumber
		{
			get
			{
				return LocalAccountDescriptor != null ? LocalAccountDescriptor.AJ_LocalAccountNumber : ZString.Empty;
			}
		}

		public ZString LocalAccountDescription
		{
			get
			{
				return LocalAccountDescriptor != null ? LocalAccountDescriptor.AJ_AccountDescription : ZString.Empty;
			}
		}

		public ZString AlternateAccounts
		{
			get
			{
				var result = string.Empty;

				var gLAccountSelectionAndEntry = AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (gLAccountSelectionAndEntry != Guid.Empty)
				{
					var query = new ZQuery();
					query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, gLAccountSelectionAndEntry);
					query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, PK);

					var selectedOnes = Factory.Load<AccAlternateGLAccountAttribute>(query).Select(x => x.AlternateGLAccount);
#if NETFRAMEWORK
					var alternateGLAccounts = selectedOnes.DistinctBy(x => x.PK);
#else
					var alternateGLAccounts = System.Linq.Enumerable.DistinctBy(selectedOnes, x => x.PK);
#endif
					result = string.Join(", ", alternateGLAccounts.Take(3).Select(x => x.AGA_AccountNum + " - " + x.AGA_Description));
					if (alternateGLAccounts.Count() > 3)
					{
						result += "...";
					}
				}

				return result;
			}
		}
	}
}
