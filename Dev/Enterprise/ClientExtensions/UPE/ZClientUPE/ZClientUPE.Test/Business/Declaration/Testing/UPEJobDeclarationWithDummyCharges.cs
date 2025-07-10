using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	public interface ITricky
	{
		void Clear();
		void Add(CustomsCharge charge);
	}

	public class UPEJobDeclarationWithDummyCharges : UPEJobDeclaration, ITricky
	{
		public UPEJobDeclarationWithDummyCharges(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ITricky CustomsCharges
		{
			get
			{
				return this;
			}
		}

		void ITricky.Clear()
		{
			fCustomsCharges = Array.Empty<CustomsCharge>();
		}

		void ITricky.Add(CustomsCharge charge)
		{
			if (fCustomsCharges == null)
			{
				fCustomsCharges = Array.Empty<CustomsCharge>();
			}

			Array.Resize(ref fCustomsCharges, fCustomsCharges.Length + 1);
			fCustomsCharges[fCustomsCharges.Length - 1] = charge;
		}

		public void SetCustomsCharges(params CustomsCharge[] customsCharges)
		{
			this.fCustomsCharges = customsCharges;
		}

		public override object GetService(Type serviceType)
		{
			if (serviceType == typeof(ICustomsCharges))
			{
				return new Bodgy(this);
			}

			return base.GetService(serviceType);
		}

		public new TestManualBillNotification ManualBillNotification
		{
			get
			{
				return (TestManualBillNotification)base.ManualBillNotification;
			}
		}

		protected override ManualBillNotification NewManualBillNotification()
		{
			return new TestManualBillNotification(this);
		}

		public ZString GetLOALogReferenceText()
		{
			return this.LOALogReferenceText;
		}

		protected override void PrintLetterOfAuthority(UPEPrintBatch currentPrintBatch, ZGuid menuItemPK)
		{
			base.PrintLetterOfAuthority(null, ZGuid.Empty);
		}

		class Bodgy : ICustomsCharges
		{
			public Bodgy(UPEJobDeclarationWithDummyCharges declaration)
			{
				this.declaration = declaration;
			}

			readonly UPEJobDeclarationWithDummyCharges declaration;
			CustomsCharge[] ICustomsCharges.GetCustomsCharges(ILogger logger)
			{
				if (declaration.fCustomsCharges == null)
				{
					OrgHeader creditor = declaration.Factory.LoadTop1<OrgHeader>(new ZQuery());
					CustomsCharge[] fCustomsCharges = new CustomsCharge[12];
					declaration.fCustomsCharges = fCustomsCharges;
					fCustomsCharges[0] = new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DutyAmount, 10, 0m, false, creditor.PK);
					fCustomsCharges[1] = new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTAmount, 20, 0m, false, creditor.PK);
					fCustomsCharges[2] = new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.WetAmount, 20, 0m, false, creditor.PK);
					fCustomsCharges[3] = new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.Woodlevy, 20, 0m, false, creditor.PK);
					fCustomsCharges[4] = new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.LCTAmount, 20, 0m, false, creditor.PK);
					fCustomsCharges[5] = new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.DeclarationProcessingCharge, 30.10m, 0m, false, creditor.PK);
					fCustomsCharges[6] = new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.AQISProcessingCharge, 6.5m, 0m, false, creditor.PK);
					// ignore this
					fCustomsCharges[7] = new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.GSTDeferred, 100000m, 0m, false, creditor.PK);
					fCustomsCharges[8] = new CustomsCharge(null, "BLAH", 30, 0m, false, creditor.PK);
					fCustomsCharges[9] = new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.OtherCharges, 200, 0m, false, creditor.PK);
					fCustomsCharges[10] = new CustomsCharge(null, CusEntryChargeTypeList.Descriptions.ScreenFree, 200, 0m, false, creditor.PK);
					fCustomsCharges[11] = new CustomsCharge(null, "BLAH AGAIN", 40, 0m, false, creditor.PK);
				}

				return declaration.fCustomsCharges;
			}

			ZBool ICustomsCharges.IsActive
			{
				get
				{
					return true;
				}
			}
		}

		#region ICustomsCharges Members
		CustomsCharge[] fCustomsCharges;
		#endregion
		public class TestManualBillNotification : ManualBillNotification
		{
			public TestManualBillNotification(UPEJobDeclaration declaration) : base(declaration)
			{
			}

			public void SetShouldDoManualBillActivitiesOnSave(bool value)
			{
				fShouldDoManualBillActivitiesOnSave = value;
			}

			public override bool ShouldDoManualBillActivitiesOnSave
			{
				get
				{
					return fShouldDoManualBillActivitiesOnSave;
				}
			}

			bool fShouldDoManualBillActivitiesOnSave;
		}
	}
}
