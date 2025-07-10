using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Integration
{
	///<Summary>
	///<value>Property <c>Country</c> represents the RefCountry</value>
	///<value>Property <c>Parent</c> represents the business object</value>
	///<value>Property <c>Parents</c> represents the list of business object</value>
	///<value>Property <c>Code</c> represents the RefCountry.RN_Code</value>
	///<value>Property <c>IsSanctioned</c> represents the RefCountry.IsSanctioned</value>
	///<value>Property <c>ParentsDescription</c> represents the description of the business object</value>
	///<value>Property <c>LocationDescription</c> represents the RefCountry.RN_Desc</value>
	///<value>Property <c>Key</c> represents the RefCountry.PK</value>
	///</Summary>
	public interface IComplianceLocation
	{
		BusinessObject Country { get; }
		BusinessObject Parent { get; }
		List<BusinessObject> Parents { get; }
		ZString Code { get; }
		ZBool IsSanctioned { get; }
		ZString ParentsDescription { get; set; }
		ZString LocationDescription { get; }
		ZGuid Key { get; }
	}
}
