using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageRegHeader), nameof(CusTempStorageRegHeader.CusTempStorageRegLines))]
	[CodeProperty(nameof(CusTempStorageRegLine.HeaderRegNo))]
	[DescriptionProperty(nameof(CusTempStorageRegLine.HeaderRegNo))]
	public class CusTempStorageRegLine : EU.TemporaryStorage.Business.CusTempStorageRegLine
		, Integration.Customs.DE.ICusTempStorageRegLine
	{
		public CusTempStorageRegLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[ResourceStringData("0474942E-C13C-4FB1-9259-2EEF17544D3C", Caption = "Line Status")]
		public override ZString SRL_CustomsStatus
		{
			get => base.SRL_CustomsStatus;
			set => base.SRL_CustomsStatus = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("D288CA2B-B625-448A-9CFD-DA184E7978E4", Caption = "Package Count")]
		public override ZInt SRL_PackagesRemaining
		{
			get => base.SRL_PackagesRemaining;
			set => base.SRL_PackagesRemaining = value;
		}

		[MaxLength(17)]
		[ResourceStringData("DF3FC34F-9497-46F6-B456-DEFBC6A59F31", Caption = "Custodian EORI")]
		public override ZString SRL_CustodianIdentifier
		{
			get => base.SRL_CustodianIdentifier;
			set => base.SRL_CustodianIdentifier = value;
		}

		[MaxLength(17)]
		[ResourceStringData("695694F1-7F69-443C-9609-7FB4938B7BAA", Caption = "Disp. Ent. Trader EORI")]
		public override ZString SRL_GoodsOwnerIdentifier
		{
			get => base.SRL_GoodsOwnerIdentifier;
			set => base.SRL_GoodsOwnerIdentifier = value;
		}

		#endregion

		public ZString HeaderRegNo => RegHeader?.SRH_Reference ?? ZString.Empty;

		public new CusTempStorageRegHeader RegHeader => (CusTempStorageRegHeader)base.RegHeader;

		public new CusTempStorageRegLineTransactionCollection CusTempStorageRegLineTransactions => (CusTempStorageRegLineTransactionCollection)base.CusTempStorageRegLineTransactions;

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionCollection CreateNewCusTempStorageRegLineTransactions()
		{
			var transactions = new CusTempStorageRegLineTransactionCollection(this);
			transactions.CountChanged += (sender, e) => UpdatePackagesRemaining();
			return transactions;
		}

		public new CusTempStorageRegLineValidation Validation => (CusTempStorageRegLineValidation)base.Validation;

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineValidation GetNewValidation() => new CusTempStorageRegLineValidation(this);

		public new CusTempStorageRegLineLookups Lookups => (CusTempStorageRegLineLookups)base.Lookups;

		protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineLookups GetNewLookups() => new CusTempStorageRegLineLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SRL_PackagesRemaining = 0;
		}

		protected override Type GetStorageRegLineTransactionCore() => typeof(CusTempStorageRegLineTransaction);
	}
}
