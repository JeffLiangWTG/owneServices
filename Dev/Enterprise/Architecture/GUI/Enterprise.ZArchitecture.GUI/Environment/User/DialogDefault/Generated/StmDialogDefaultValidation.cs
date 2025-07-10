//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmDialogDefaultValidation
//
//    This class should be used for overriding validation in AutoStmDialogDefaultValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.DialogDefault
{
	public class StmDialogDefaultValidation : AutoStmDialogDefaultValidation
	{
		public StmDialogDefaultValidation(AutoStmDialogDefault parent)
			: base(parent)
		{
		}

		protected override void CheckSDD_SerializedDefaults()
		{
			base.CheckSDD_SerializedDefaults();

			if (Parent.SDD_SerializedDefaultsInfo.HasChanges)
			{
				Parent.SDD_SerializedDefaultsInfo.AddWarning(Res.GetString("C8E22CBC-26DB-4D91-BBF9-357D5EA033DC", "Modifying the XML may result in unexpected behavior; If the software cannot determine then the desired response from the XML the default will not be used."));

				if (!IsValidXml(Parent.SDD_SerializedDefaults))
				{
					Parent.SDD_SerializedDefaultsInfo.AddError(Res.GetString("58193BD1-FB50-443C-A530-629CF2269F9D", "XML is invalid"));
				}
			}
		}

		protected override void CheckSDD_Owner()
		{
			base.CheckSDD_Owner();
			AddErrorIfDefaultWithSameOwnerAndIdentifierExists(Parent.SDD_OwnerInfo);

			if (Parent.SDD_Level != DialogDefaultLevel.Codes.Global && Parent.SDD_Owner == ZGuid.Empty)
			{
				Parent.SDD_OwnerInfo.AddError(Res.GetString("21D081D5-85C5-4311-AAB2-321377E7A563", "You must have an owner for this dialog default level. Please add an owner."));
			}
		}

		protected override void CheckSDD_Level()
		{
			base.CheckSDD_Level();
			AddErrorIfDefaultWithSameOwnerAndIdentifierExists(Parent.SDD_LevelInfo);
		}

		void AddErrorIfDefaultWithSameOwnerAndIdentifierExists(ZPropertyInfo placeToAddErrorMessage)
		{
			var duplicateQuery = new ZQuery(StmDialogDefaultSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK)
				.AddToFilter(StmDialogDefaultSchema.SDD_DialogIdentifier, Parent.SDD_DialogIdentifier)
				.AddToFilter(StmDialogDefaultSchema.SDD_Level, Parent.SDD_Level);

			if (Parent.SDD_Level != DialogDefaultLevel.Codes.Global)
			{
				duplicateQuery.AddToFilter(StmDialogDefaultSchema.SDD_Owner, Parent.SDD_Owner);
			}

			if (Parent.SDD_Context.IsEmpty)
			{
				duplicateQuery.AddToFilter(StmDialogDefaultSchema.SDD_Context, null);
			}
			else
			{
				var contextQuery = new ZDBOnlyQuery(typeof(StmDialogDefault));
				contextQuery.AddToFilter(StmDialogDefaultSchema.SDD_Context, Parent.SDD_Context);
				duplicateQuery.AddToFilter(contextQuery);
			}

			if (Parent.Factory.LoadTop1<StmDialogDefault>(duplicateQuery) != null)
			{
				placeToAddErrorMessage.AddError(Res.GetString("C589CAC2-40F7-4746-9DD0-A8D4FF64D080", "There is another default with the same identifier, owner and context. Please change the owner of either default, or delete the other default"));
			}
		}

		bool IsValidXml(string xml)
		{
			try
			{
				XElement.Parse(xml);
				return !string.IsNullOrWhiteSpace(xml);
			}
			catch (XmlException)
			{
				return false;
			}
		}
	}
}
