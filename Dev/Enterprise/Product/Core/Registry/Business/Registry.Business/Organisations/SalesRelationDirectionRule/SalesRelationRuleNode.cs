using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public class SalesRelationRuleNode : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SalesRelationRuleNode()
			: base()
		{
		}

		#region Type

		[List("Lookups.AllActivityTypes")]
		[MaxLength(3)]
		public ZString Type
		{
			get { return type; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(TypeInfo, ref this.type, value);
				if (!IsValidationSuspended)
				{
					ValidateType();
				}
			}
		}
		ZString type;

		public ZPropertyInfo TypeInfo
		{
			get { return GetZPropertyInfo(nameof(Type)); }
		}

		public void ValidateType()
		{
			TypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TypeInfo);
			ListValidation.ErrorIfInvalidCode(TypeInfo);

			if (Type == SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities)
			{
				var parentCollections = ((IBusinessObjectInternals)this).ParentCollections.OfType<SalesRelationRuleNodeCollection>();
				if (parentCollections.Any(collection => !collection.IsLastNode(this)))
				{
					TypeInfo.AddError(Res.GetString("679d32d2-906b-481c-93b6-385521a90951", "A type of * can only be at the end of a sequence."));
				}
			}
		}

		#endregion

		#region TypeDescription

		public ZString TypeDescription
		{
			get { return Lookups.AllActivityTypes.GetDescriptionFromCode(Type); }
		}

		public ZPropertyInfo TypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TypeDescription)); }
		}

		#endregion

		#region Lookups

		public SalesRelationRuleNodeLookups Lookups
		{
			get { return lookups ?? (lookups = new SalesRelationRuleNodeLookups(this)); }
		}
		SalesRelationRuleNodeLookups lookups;

		#endregion
	}
}
