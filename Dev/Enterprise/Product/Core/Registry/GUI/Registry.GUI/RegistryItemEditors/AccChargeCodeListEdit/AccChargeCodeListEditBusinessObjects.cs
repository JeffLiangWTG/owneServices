using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	#region AccChargeCodeListElement

	public class AccChargeCodeListElement : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string ChargeCode = "ChargeCode";
			public const string ChargeDescription = "ChargeDescription";
		}

		#endregion

		public AccChargeCodeListElement(ZGuid chargeCodePK, AccChargeCodeListCollection parent)
		{
			fChargeCode = chargeCodePK;
			this.Parent = parent;
		}

		protected AccChargeCodeListCollection Parent;

		#region Collections

		public IBusinessObjectCollection AccChargeCodeCollection
		{
			get { return Parent.AccChargeCodeCollection; }
		}

		#endregion

		#region Properties

		#region Charge Code

		protected ZGuid fChargeCode;
		public ZGuid ChargeCode
		{
			get { return fChargeCode; }
			set
			{
				fChargeCode = value;
				if (!IsValidationSuspended)
				{
					ValidateChargeCode();
				}
				ChargeCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ChargeCodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ChargeCode); }
		}

		void ValidateChargeCode()
		{
			ChargeCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ChargeCodeInfo);
			TypeValidation.CheckValidGuid(ChargeCodeInfo);
			ListValidation.ErrorIfInvalidPK(ChargeCodeInfo, AccChargeCodeCollection);
			if (!ChargeCodeInfo.HasErrors())
			{
				foreach (AccChargeCodeListElement chargeCodeListElement in Parent)
				{
					if (chargeCodeListElement.PK != PK && chargeCodeListElement.ChargeCode == ChargeCode)
					{
						ChargeCodeInfo.AddError(Res.GetString("9ebd6a7f-0194-4878-ad2a-1c35737e2090", "There must be only one the same charge code in the list."));
						break;
					}
				}
			}
		}

		#endregion

		#region Charge Description

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public ZString ChargeDescription
		{
			get
			{
				var charge = Parent.Factory.Load<AccChargeCode>(ChargeCode);
				return charge != null ? charge.AC_Desc : ZString.Empty;
			}
		}

		public ZPropertyInfo ChargeDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.ChargeDescription); }
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateChargeCode();
		}

		#endregion
	}

	#endregion

	#region AccChargeCodeListCollection

	public class AccChargeCodeListCollection : NonPersistentBusinessObjectCollection<AccChargeCodeListElement>	{
		public AccChargeCodeListCollection(string value, RegistryFindBoxFilter filter, BusinessObjectFactory factory, Guid companyPK) : base(factory)
		{
			this.Filter = filter;
			this.CompanyPK = companyPK;
			Load(value);
		}

		public void Load(string value)
		{
			var emptyGuidString = Guid.Empty.ToString();
			var chargeCodeGuidStrings = value.Split(',');

			foreach (string chargeCodeGuidString in chargeCodeGuidStrings)
			{
				try
				{
					if (!string.IsNullOrEmpty(chargeCodeGuidString) && chargeCodeGuidString != emptyGuidString)
					{
						var chargeCodeGuid = new ZGuid(chargeCodeGuidString);
						if (chargeCodeGuid.IsValid && !chargeCodeGuid.IsEmpty)
						{
							if (CompanyPK != Guid.Empty)
							{
								chargeCodeGuid = ChangeGlobalPKToLocalPK(chargeCodeGuid);
							}
							if (!chargeCodeGuid.IsEmpty)
							{
								Add(new AccChargeCodeListElement(chargeCodeGuid, this));
							}
						}
					}
				}
				catch (ArgumentNullException)
				{
				}
				catch (FormatException)
				{
				}
			}
		}

		/// <summary>
		/// Converts a global charge code to a local charge code. Required because if you override the global default,
		/// we cannot use the global charge code PKs as they are invalid for the colleciton filter used which specifies the company.
		/// </summary>
		/// <param name="chargeCodeGuid"></param>
		/// <returns></returns>
		ZGuid ChangeGlobalPKToLocalPK(ZGuid chargeCodeGuid)
		{
			var resultChargeCodeGuid = ZGuid.Empty;
			using (var cmd = CargoWise.Data.Db.Connection.Command(@"SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_Code = (SELECT AC_Code FROM dbo.AccChargeCode WHERE AC_PK = @PK) AND AC_GC = @GC")) // Explicit SQL reduces number of hits 
			{
				cmd.AddParameter("@PK", System.Data.SqlDbType.UniqueIdentifier, chargeCodeGuid.ToGuid());
				cmd.AddParameter("@GC", System.Data.SqlDbType.UniqueIdentifier, CompanyPK);
				var reader = cmd.ExecuteReader();

				if (reader.Read())
				{
					var readerChargeCodePK = reader[AccChargeCodeSchema.PK.Name];
					if (readerChargeCodePK != DBNull.Value)
					{
						resultChargeCodeGuid = new ZGuid(readerChargeCodePK);
					}
				}

				reader.Close();
			}

			return resultChargeCodeGuid;
		}

		public override string ToString()
		{
			string result = "";
			foreach (AccChargeCodeListElement elem in this)
			{
				if (elem.ChargeCode.IsValid && !elem.ChargeCode.IsEmpty)
				{
					if (!string.IsNullOrEmpty(result))
					{
						result += ",";
					}

					result += elem.ChargeCode.ToString();
				}
			}

			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AccChargeCodeListElement(ZGuid.Empty, this);
		}

		public IBusinessObjectCollection AccChargeCodeCollection
		{
			get
			{
				if (fAccChargeCodeCollection == null)
				{
					ZQuery filter = GetAdditionalFilterQuery();
					if (CompanyPK == ZGuid.Empty)
					{
						fAccChargeCodeCollection = new AccGlobalChargeCodeCollection(Factory, filter);
					}
					else
					{
						fAccChargeCodeCollection = new AccChargeCodeCollection(Factory, filter, CompanyPK);
					}
				}
				return fAccChargeCodeCollection;
			}
		}

		ZQuery GetAdditionalFilterQuery()
		{
			ZQuery query = new ZQuery();
			switch (Filter)
			{
				case RegistryFindBoxFilter.FreightChargeCode:
					query = new ZQuery(AccChargeCodeSchema.AC_ChargeGroup, "FRT");
					break;
				case RegistryFindBoxFilter.MrgDsbOrMjaChargeCode:
					query = new ZQuery(AccChargeCodeSchema.AC_ChargeType, "DSB");
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, "MRG");
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, "MJA");
					break;
			}

			query.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_IsActive, true);
			if (CompanyPK != ZGuid.Empty)
			{
				query.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_GC, CompanyPK);
			}
			return query;
		}

		protected IBusinessObjectCollection fAccChargeCodeCollection;
		internal readonly RegistryFindBoxFilter Filter;
		internal readonly Guid CompanyPK;
	}

	#endregion

	#region AccChargeCodeListCollectionWrapper

	public class AccChargeCodeListCollectionWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AccChargeCodeListCollectionWrapper(string value, RegistryFindBoxFilter filter, BusinessObjectFactory factory, Guid companyPK)
		{
			fAccChargeCodeList = new AccChargeCodeListCollection(value, filter, factory, companyPK);
			RegisterEditableChildObject(fAccChargeCodeList);
		}

		public AccChargeCodeListCollection AccChargeCodeList
		{
			get { return fAccChargeCodeList; }
		}

		readonly AccChargeCodeListCollection fAccChargeCodeList;
	}

	#endregion
}
