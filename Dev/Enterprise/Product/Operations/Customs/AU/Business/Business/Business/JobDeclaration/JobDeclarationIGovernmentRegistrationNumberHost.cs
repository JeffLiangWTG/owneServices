//using System;
//using System.Collections.Generic;
//using System.Text;
//using Enterprise.MasterFiles.Business;
//using Enterprise.ZArchitecture;

//namespace Enterprise.Customs.AU.Declaration.Business
//{
//  public partial class JobDeclaration : TypeSafeJobDeclaration, IGovernmentRegistrationNumberHost
//  {
//    #region IGovernmentRegistrationNumberHost Members

//    RegistrationNumberResult IGovernmentRegistrationNumberHost.GetRegistrationNumberResult(OrgAddress address, DocAddressType addressType)
//    {
//      string[] mappingCodes = null;
//      switch (addressType)
//      {
//        default: mappingCodes = null; break;
////				case DocAddressType.SupplierPickupDeliveryAddress: mappingCodes = new string[] { "SSN", "EIN", "ABN", "CCP" }; break;
//      }

//      return new RegistrationNumberResult(mappingCodes != null, 
//        delegate
//        {
//          ZString result;
//          if (address != null)
//          {
//            OrgCusCodeCollection codes = address.Header.CustomsCodes;
//            foreach (string mappingCode in mappingCodes)
//            {
//              result = codes.GetCustomsRegNo(mappingCode, Core.Constants.CountryGuids.Australia);
//              if (!result.IsEmpty)
//              {
//                break;
//              }
//            }
//          }
//          return result;
//        });
//    }

//    #endregion
//  }
//}

//namespace Enterprise.Customs.AU.Declaration.Business.Testing
//{
//  using Enterprise.ZArchitecture.Business.Testing;
//#if DEBUG
//#else
//#error DON't release my burp!
//#endif

//  public class JobDeclarationIGovernmentRegistrationNumberHostTest : TestCaseWithFactory
//  {
//    public void TestGetRegistrationNumber()
//    {
//      IGovernmentRegistrationNumberHost host = (IGovernmentRegistrationNumberHost)Factory.New<JobDeclaration>();
//      AssertEquals("BURP!", host.GetRegistrationNumberResult(Factory.New<OrgAddress>(), DocAddressType.PickUpAddress));
//    }
//  }
//}
