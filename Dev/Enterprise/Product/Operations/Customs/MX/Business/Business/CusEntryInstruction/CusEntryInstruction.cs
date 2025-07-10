using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MX;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.MX.Business
{
	[CodeProperty(CusEntryInstruction.Schema.CEI_Description)]
	public class CusEntryInstruction : Customs.Business.CusEntryInstruction, Integration.Customs.MX.ICusEntryInstruction, ICusSupportingInfoTypeSupporter
	{
		public CusEntryInstruction(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoCusEntryInstruction.Schema
		{
			public const int UCRNumberMaxLength = 7;
			public const string UCRNumber = "UCRNumber";
		}

		#endregion

		#region UCR Details

		CusEntryNumber UCRCusEntryNumber => Factory.GetValue(ref fUCRCusEntryNumber, () =>
					{
						var ucrEntryNum = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Standard.UniqueConsignementReference, Core.Constants.CountryCodes.Mexico);
						RegisterEditableChildObject(ucrEntryNum);
						return ucrEntryNum;
					});

		CachedProperty<CusEntryNumber> fUCRCusEntryNumber;

		[MaxLength(Schema.UCRNumberMaxLength)]
		[ReadOnlyMember(nameof(UCRNumber_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.MX.Business.CusEntryInstruction|UCRNumber", Caption = "Entry Number")]
		public ZString UCRNumber
		{
			get { return UCRCusEntryNumber.CE_EntryNum; }
			set
			{
				if (UCRNumber != value)
				{
					CheckMaximumLength(UCRNumberInfo, value);
					UCRCusEntryNumber.CE_EntryNum = value;
				}
				UCRNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UCRNumberInfo => GetZPropertyInfo(Schema.UCRNumber);

		bool UCRNumber_ReadOnly => true;

		#endregion

		#region Identifier

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public IdentifierCollection Identifiers
		{
			get
			{
				if (fIdentifierCollection == null)
				{
					fIdentifierCollection = new IdentifierCollection(this);
					fIdentifierCollection.Load();
					RegisterEditableChildObject(fIdentifierCollection);
				}
				return fIdentifierCollection;
			}
		}

		IdentifierCollection fIdentifierCollection;

		#endregion

		#region Clearance

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public ClearanceCollection Clearances
		{
			get
			{
				if (fClearanceCollection == null)
				{
					fClearanceCollection = new ClearanceCollection(this);
					fClearanceCollection.Load();
					RegisterEditableChildObject(fClearanceCollection);
				}
				return fClearanceCollection;
			}
		}

		ClearanceCollection fClearanceCollection;

		#endregion

		public override void Delete()
		{
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			Factory.Load<CusEntryNumber>(filter).DeleteAll();
			base.Delete();
		}

		public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ CusSupportingInfoTypeList.Codes.Clearance, typeof(Clearance) },
				{ CusSupportingInfoTypeList.Codes.Identifier, typeof(Identifier) }
			};
			return result;
		}

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}
	}
}
