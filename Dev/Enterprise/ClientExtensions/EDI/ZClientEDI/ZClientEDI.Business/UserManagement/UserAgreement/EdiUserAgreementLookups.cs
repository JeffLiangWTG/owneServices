//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiUserAgreementLookups
//
//    This class should be used for overriding collections in AutoEdiUserAgreementLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementLookups : AutoEdiUserAgreementLookups
	{
		public EdiUserAgreementLookups(AutoEdiUserAgreement parent) : base(parent)
		{
			this.parent = parent;
		}

		#region Types

		public ReadOnlyCodeDescriptionPairList Types => Factory.GetCachedValue<EdiUserAgreementTypes>();

		#endregion

		readonly AutoEdiUserAgreement parent;

		#region Levels

		public ReadOnlyCodeDescriptionPairList Levels => new EdiUserAgreementLevelList();

		#endregion

		#region Variant

		public ReadOnlyCodeDescriptionPairList Variants => GetVariantList(Factory, parent.ERA_Type);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static CodeDescriptionPairList GetVariantList(BusinessObjectFactory factory, string agreementType)
		{
			if (!EdiUserAgreementTypesMapper.IsVariantEnabled(agreementType))
			{
				return new CodeDescriptionPairList();
			}

			return factory.GetCachedValue($"UserAgreementVariantList_{agreementType}", () =>
			{
				var searchQuery = $@"
SELECT DISTINCT
	{EdiUserAgreementSchema.Constants.ERA_VariantCode} as Code,
	{EdiUserAgreementSchema.Constants.ERA_VariantDescription} as Description
FROM
	dbo.EdiUserAgreement
WHERE
	{EdiUserAgreementSchema.Constants.ERA_Type} = @agreementType AND
	{EdiUserAgreementSchema.Constants.ERA_IsActive} = @isActive AND
	{EdiUserAgreementSchema.Constants.ERA_RN_NKCountryCode} = @countryCode AND
	{EdiUserAgreementSchema.Constants.ERA_VariantCode} IS NOT NULL AND
	{EdiUserAgreementSchema.Constants.ERA_VariantCode} != ''
ORDER BY
	{EdiUserAgreementSchema.Constants.ERA_VariantCode} ASC";

				var list = new CodeDescriptionPairList();
				using (var command = Db.Connection.Command(searchQuery))
				{
					command.AddParameter("@agreementType", SqlDbType.VarChar, agreementType);
					command.AddParameter("@isActive", SqlDbType.Bit, true);
					command.AddParameter("@countryCode", SqlDbType.VarChar, string.Empty);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var code = reader["Code"].ToString();
							var description = reader["Description"].ToString();
							if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(description))
							{
								list.AddPairIfNotExist(code, description);
							}
						}
					}
				}

				return list;
			},
			CacheStalenessPolicy.StaleOnFactorySave);
		}

		#endregion
	}
}
