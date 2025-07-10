using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class AssignedStaffWrapperCollection : GenericWrapperCollection<AssignedStaffWrapper>
	{
		public AssignedStaffWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AssignedStaffWrapperCollection(OrgHeader header, BusinessObjectFactory factory)
			: this(factory)
		{
			lookup = new Dictionary<string, AssignedStaffWrapper>(StringComparer.OrdinalIgnoreCase);

			if (header != null)
			{
				OrgStaffAssignmentsCollection assignments = header.StaffAssignments;
				AddSalesRepresentatives(assignments);
				AddCustomerServiceAgents(assignments);
				AddCartageCoordinators(assignments);
			}
		}

		protected override IBODocDataProvider GetRow(ZString index)
		{
			AssignedStaffWrapper result;

			if (lookup != null && lookup.TryGetValue(index, out result))
			{
				return result;
			}
			else
			{
				return base.GetRow(index);
			}
		}

		void AddSalesRepresentatives(OrgStaffAssignmentsCollection assignments)
		{
			const string category = StaffAssignmentRoles.Codes.SalesRep;
			AddStaffConditionally(assignments.OverallSalesRepStaff, null, category, category, CommonResourceStrings.OverallRepresentative);
			AddStaffConditionally(assignments.ImportAirRepStaff, assignments.OverallSalesRepStaff, category, category + ImpAir, CommonResourceStrings.ImportAirRepresentative);
			AddStaffConditionally(assignments.ImportSeaRepStaff, assignments.OverallSalesRepStaff, category, category + ImpSea, CommonResourceStrings.ImportSeaRepresentative);
			AddStaffConditionally(assignments.ExportAirRepStaff, assignments.OverallSalesRepStaff, category, category + ExpAir, CommonResourceStrings.ExportAirRepresentative);
			AddStaffConditionally(assignments.ExportSeaRepStaff, assignments.OverallSalesRepStaff, category, category + ExpSea, CommonResourceStrings.ExportSeaRepresentative);
			AddStaffConditionally(assignments.WarehousingRepStaff, assignments.OverallSalesRepStaff, category, category + Whs, CommonResourceStrings.WarehousingRepresentative);
		}
		void AddCustomerServiceAgents(OrgStaffAssignmentsCollection assignments)
		{
			const string category = StaffAssignmentRoles.Codes.CustomerServiceRep;
			AddStaffConditionally(assignments.OverallCustomerServiceRepStaff, null, category, category, CommonResourceStrings.OverallCustomerServices);
			AddStaffConditionally(assignments.ImportAirCustomerServiceRepStaff, assignments.OverallCustomerServiceRepStaff, category, category + ImpAir, CommonResourceStrings.ImportAirCustomerServices);
			AddStaffConditionally(assignments.ImportSeaCustomerServiceRepStaff, assignments.OverallCustomerServiceRepStaff, category, category + ImpSea, CommonResourceStrings.ImportSeaCustomerServices);
			AddStaffConditionally(assignments.ExportAirCustomerServiceRepStaff, assignments.OverallCustomerServiceRepStaff, category, category + ExpAir, CommonResourceStrings.ExportAirCustomerServices);
			AddStaffConditionally(assignments.ExportSeaCustomerServiceRepStaff, assignments.OverallCustomerServiceRepStaff, category, category + ExpSea, CommonResourceStrings.ExportSeaCustomerServices);
		}
		void AddCartageCoordinators(OrgStaffAssignmentsCollection assignments)
		{
			const string category = StaffAssignmentRoles.Codes.CartageCoordinator;
			AddStaffConditionally(assignments.OverallCartageCoordinatorStaff, null, category, category, CommonResourceStrings.OverallLocalTransportCoordinator);
			AddStaffConditionally(assignments.ImportAirCartageCordinatorStaff, assignments.OverallCartageCoordinatorStaff, category, category + ImpAir, CommonResourceStrings.ImportAirLocalTransportCoordinator);
			AddStaffConditionally(assignments.ImportSeaCartageCordinatorStaff, assignments.OverallCartageCoordinatorStaff, category, category + ImpSea, CommonResourceStrings.ImportSeaLocalTransportCoordinator);
			AddStaffConditionally(assignments.ExportAirCartageCordinatorStaff, assignments.OverallCartageCoordinatorStaff, category, category + ExpAir, CommonResourceStrings.ExportAirLocalTransportCoordinator);
			AddStaffConditionally(assignments.ExportSeaCartageCordinatorStaff, assignments.OverallCartageCoordinatorStaff, category, category + ExpSea, CommonResourceStrings.ExportSeaLocalTransportCoordinator);
		}

		void AddStaffConditionally(GlbStaff staffToAdd, GlbStaff overallStaff, string category, string key, string relationship)
		{
			if (staffToAdd != null)
			{
				AssignedStaffWrapper wrapper = new AssignedStaffWrapper(staffToAdd, category, relationship, Factory);
				lookup.Add(key, wrapper);

				if (staffToAdd != overallStaff)
				{
					Add(wrapper);
				}
			}
			else if (overallStaff != null)
			{
				AssignedStaffWrapper wrapper = new AssignedStaffWrapper(overallStaff, category, relationship, Factory);
				lookup.Add(key, wrapper);
			}
		}

		const string ImpAir = "-IMP-AIR";
		const string ImpSea = "-IMP-SEA";
		const string ExpAir = "-EXP-AIR";
		const string ExpSea = "-EXP-SEA";
		const string Whs = "-WHS";

		readonly Dictionary<string, AssignedStaffWrapper> lookup;
	}
}
