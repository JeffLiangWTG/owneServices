using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentResourceLinkValidation : AutoBMComponentResourceLinkValidation
	{
		public BMComponentResourceLinkValidation(AutoBMComponentResourceLink parent)
			: base(parent)
		{
		}

		new BMComponentResourceLink Parent
		{
			get { return (BMComponentResourceLink)base.Parent; }
		}

		protected override void CheckFD_GS_NKResource()
		{
			base.CheckFD_GS_NKResource();
			MandatoryValidation.CheckEntered(Parent.FD_GS_NKResourceInfo);
			CheckForeignKeysAreUnique(Parent.FD_GS_NKResourceInfo);
		}

		protected override void CheckFD_FC_Component()
		{
			base.CheckFD_FC_Component();
			CheckForeignKeysAreUnique(Parent.FD_FC_ComponentInfo);
		}

		protected override void CheckFD_CapacityLimitPercent()
		{
			base.CheckFD_CapacityLimitPercent();

			if (Parent.Component != null && Parent.Component.IsBuffer)
			{
				CompareValidation.CheckLessThanOrEqualTo(Parent.FD_CapacityLimitPercentInfo, 100m);
			}
			else if (Parent.FD_CapacityLimitPercent > 0)
			{
				Parent.FD_CapacityLimitPercentInfo.AddError(Res.GetString("73cb5639-d78b-4bef-af09-3aba53083b01", "A Capacity Limit Percent is only valid on a Buffer component."));
			}
		}

		void CheckForeignKeysAreUnique(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsEmpty)
			{
				var query = GetQueryForForeignKeyValidation(Parent);
				var otherLink = Parent.Factory.Load<BMComponentResourceLink>(query);

				if (otherLink.Length > 0)
				{
					propertyInfo.AddError(Res.GetString("f350825d-a2b5-4266-a817-10c3ef2abcb7", "This combination of component and resource has been duplicated."));
				}
			}
		}

		protected override void CheckFD_IsCapacityConstrained()
		{
			base.CheckFD_IsCapacityConstrained();

			if (Parent.FD_IsCapacityConstrained)
			{
				var buffer = Parent.Component;
				if (buffer != null)
				{
					var hasConstraint = buffer.ChildComponents.Any(c => c.FC_Type == BMComponentTypeList.Codes.Constraint);
					if (!hasConstraint)
					{
						Parent.FD_IsCapacityConstrainedInfo.AddError(Res.GetString("4cd19dc1-854e-46c7-a227-9f7bd9ff4c48", "A resource can only be marked as constrained when there is a Constraint sub-component of the selected Buffer."));
					}
				}
			}
		}

		internal static ZQuery GetQueryForForeignKeyValidation(BMComponentResourceLink link)
		{
			var query = new ZQuery(BMComponentResourceLinkSchema.FD_FC_Component, link.FD_FC_Component);
			query.AddToFilter(BMComponentResourceLinkSchema.FD_GS_NKResource, link.FD_GS_NKResource);
			query.AddToFilter(BMComponentResourceLinkSchema.PK, SQLComparisonOperator.NotEqual, link.PK);

			return query;
		}
	}
}
