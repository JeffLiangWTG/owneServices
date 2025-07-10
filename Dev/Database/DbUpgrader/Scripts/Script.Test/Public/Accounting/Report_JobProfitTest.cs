using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(Report_JobProfit))]
	class Report_JobProfitTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "exec sp_executesql N'EXEC Report_JobProfit @IncludeJobDetails = ''Y'', @JobType = @p727, @GroupByJob = ''Y'', @JH_GC = @p728, @JH_Status = @p729, @JH_IsActive = @p897, @JH_BranchPKList = @p730,  @JH_departmentpkList = @p731 , @JH_SalesRepPK = @p732, @JH_OperatorPK = @p733 , @JH_FromCreatedDate = @p734 , @JH_ToCreatedDate = @p735, @JH_FromClosedDate = @p736 , @JH_ToClosedDate = @p737, @JH_FromRevenueRecognizedDate = @p738 , @JH_ToRevenueRecognizedDate = @p739, @JH_LocalClientPKList = @p740, @JH_OverseasAgentPKList =  @p741, @AC_ChargeGroup =  @p742 , @AL_BranchPKList = @p743, @AL_departmentpkList = @p744, @AL_ChargeCodePKList = @p745, @AL_CreditorPKList = @p746, @AL_DebtorPKList = @p747, @AL_FromDate = @p748, @AL_ToDate = @p749, @JobShipmentList = @p751, @JobDeclarationList = @p751, @JS_TransportMode =@p752 , @JE_TransportMode =@p753, @JS_ContainerMode = @p754, @JE_ContainerMode = @p755, @FW_OriginPK =@p756 , @FW_DestinationPK = @p757, @FW_FromETA = @p758, @FW_ToETA = @p759, @FW_FromETD = @p760, @FW_ToETD = @p761, @AL_OutstandingWIPOnly = @p762, @AL_OutstandingACROnly = @p763, @JobConsolList = @p765, @JK_ContainerMode =@p766  , @JK_TransportMode =@p767 , @JK_AgentType =@p768 , @JK_SendingAgentPK =@p769 , @JK_ReceivingAgentPK =@p770 , @JK_ColoadAgentPK =@p771 , @JK_CarrierPK =@p772 , @JK_JX_LoadPort =@p773 , @JK_JX_DischargePort =@p774 , @JK_JX_FromETD =@p775 , @JK_JX_ToETD =@p776 , @JK_JX_FromETA =@p777 , @JK_JX_ToETA =@p778 , @CurrentCountry = @p779, @FW_FromRegistration = @p780, @FW_ToRegistration = @p781, @AC_AR_SalesGroup = @p782, @AC_AR_ExpenseGroup = @p783, @Gateway = @p896',N'@p727 nvarchar(4000),@p728 uniqueidentifier,@p729 nvarchar(4000),@p730 nvarchar(4000),@p731 nvarchar(4000),@p732 nvarchar(4000),@p733 nvarchar(4000),@p734 nvarchar(4000),@p735 nvarchar(4000),@p736 nvarchar(4000),@p737 nvarchar(4000),@p738 nvarchar(4000),@p739 nvarchar(4000),@p740 nvarchar(4000),@p741 nvarchar(4000),@p742 nvarchar(4000),@p743 nvarchar(4000),@p744 nvarchar(4000),@p745 nvarchar(4000),@p746 nvarchar(2222),@p747 nvarchar(2144),@p748 datetime,@p749 datetime,@p751 nvarchar(4000),@p752 nvarchar(4000),@p753 nvarchar(4000),@p754 nvarchar(4000),@p755 nvarchar(4000),@p756 nvarchar(4000),@p757 nvarchar(4000),@p758 nvarchar(4000),@p759 nvarchar(4000),@p760 nvarchar(4000),@p761 nvarchar(4000),@p762 nvarchar(4000),@p763 nvarchar(4000),@p765 nvarchar(4000),@p766 nvarchar(4000),@p767 nvarchar(4000),@p768 nvarchar(4000),@p769 nvarchar(4000),@p770 nvarchar(4000),@p771 nvarchar(4000),@p772 nvarchar(4000),@p773 nvarchar(4000),@p774 nvarchar(4000),@p775 datetime,@p776 datetime,@p777 datetime,@p778 datetime,@p779 nvarchar(2),@p780 nvarchar(4000),@p781 nvarchar(4000),@p782 nvarchar(4000),@p783 nvarchar(4000),@p784 uniqueidentifier,@p785 uniqueidentifier,@p786 uniqueidentifier,@p787 uniqueidentifier,@p788 uniqueidentifier,@p789 uniqueidentifier,@p790 uniqueidentifier,@p791 uniqueidentifier,@p792 uniqueidentifier,@p793 uniqueidentifier,@p794 uniqueidentifier,@p795 uniqueidentifier,@p796 uniqueidentifier,@p797 uniqueidentifier,@p798 uniqueidentifier,@p799 uniqueidentifier,@p800 uniqueidentifier,@p801 uniqueidentifier,@p802 uniqueidentifier,@p803 uniqueidentifier,@p804 uniqueidentifier,@p805 uniqueidentifier,@p806 uniqueidentifier,@p807 uniqueidentifier,@p808 uniqueidentifier,@p809 uniqueidentifier,@p810 uniqueidentifier,@p811 uniqueidentifier,@p812 uniqueidentifier,@p813 uniqueidentifier,@p814 uniqueidentifier,@p815 uniqueidentifier,@p816 uniqueidentifier,@p817 uniqueidentifier,@p818 uniqueidentifier,@p819 uniqueidentifier,@p820 uniqueidentifier,@p821 uniqueidentifier,@p822 uniqueidentifier,@p823 uniqueidentifier,@p824 uniqueidentifier,@p825 uniqueidentifier,@p826 uniqueidentifier,@p827 uniqueidentifier,@p828 uniqueidentifier,@p829 uniqueidentifier,@p830 uniqueidentifier,@p831 uniqueidentifier,@p832 uniqueidentifier,@p833 uniqueidentifier,@p834 uniqueidentifier,@p835 uniqueidentifier,@p836 uniqueidentifier,@p837 uniqueidentifier,@p838 uniqueidentifier,@p839 uniqueidentifier,@p840 uniqueidentifier,@p841 uniqueidentifier,@p842 uniqueidentifier,@p843 uniqueidentifier,@p844 uniqueidentifier,@p845 uniqueidentifier,@p846 uniqueidentifier,@p847 uniqueidentifier,@p848 uniqueidentifier,@p849 uniqueidentifier,@p850 uniqueidentifier,@p851 uniqueidentifier,@p852 uniqueidentifier,@p853 uniqueidentifier,@p854 uniqueidentifier,@p855 uniqueidentifier,@p856 uniqueidentifier,@p857 uniqueidentifier,@p858 uniqueidentifier,@p859 uniqueidentifier,@p860 uniqueidentifier,@p861 uniqueidentifier,@p862 uniqueidentifier,@p863 uniqueidentifier,@p864 uniqueidentifier,@p865 uniqueidentifier,@p866 uniqueidentifier,@p867 uniqueidentifier,@p868 uniqueidentifier,@p869 uniqueidentifier,@p870 uniqueidentifier,@p871 uniqueidentifier,@p872 uniqueidentifier,@p873 uniqueidentifier,@p874 uniqueidentifier,@p875 uniqueidentifier,@p876 uniqueidentifier,@p877 uniqueidentifier,@p878 uniqueidentifier,@p879 uniqueidentifier,@p880 uniqueidentifier,@p881 uniqueidentifier,@p882 uniqueidentifier,@p883 uniqueidentifier,@p884 uniqueidentifier,@p885 uniqueidentifier,@p886 uniqueidentifier,@p887 uniqueidentifier,@p888 uniqueidentifier,@p889 uniqueidentifier,@p890 uniqueidentifier,@p891 uniqueidentifier,@p892 uniqueidentifier,@p893 uniqueidentifier,@p894 uniqueidentifier,@p895 uniqueidentifier, @p896 nvarchar(3), @p897 nvarchar(8)',  @p727=N'',@p728='B651EAE1-D86C-4715-B0E5-A748DEF329DF',@p729=N'',@p730=N'',@p731=N'',@p732=NULL,@p733=NULL,@p734=N'',@p735=N'',@p736=N'',@p737=N'',@p738=N'',@p739=N'',@p740=N'',@p741=N'',@p742=N'',@p743=N'',@p744=N'',@p745=N'',@p746=N'''6721c3fa-bcf5-4839-a444-40ef6492f214'',''c79c760d-39bf-4b0c-bf09-e069ba51359c'',''5ae3930e-4acb-4bb7-80dc-61a4469cee0d'',''7fe0014e-173c-4ef1-9121-c3dbff48e514'',''953ce83b-3d2b-476c-b139-42e28abd3e5e'',''038b5ab8-4e06-4961-8a67-2493ae2c7d73'',''45748a48-713a-4155-b7db-3b8e87c2782e'',''159d9b27-4537-4ea1-a632-4b18b4ad632c'',''b2ae038f-7735-4edf-8186-4cb59fcc7e3f'',''8d49a266-1467-4d90-b25d-a6dfa74047db'',''c80b8cc5-625b-4c2c-bcdf-3b3e3e697d05'',''3df7c0ae-e5ec-4dbe-8622-2e916b380fc8'',''fdccb3a4-a093-445c-814f-fb1c0d1d97d4'',''ab08248f-6d48-4ade-b11c-cc1f6c369a8f'',''910e1bff-5065-4de7-a3ac-4a63fdc5904a'',''59f114c8-5f9f-4049-ad30-aa1e81667559'',''06889c07-102b-4dfe-be0a-9b25caffd48d'',''9d690b7b-f9cc-4d64-8013-3cf3df47effb'',''3c9cd1ba-e697-4d82-b21d-241adfb1f8d0'',''cd8f6cf6-96dc-4161-a22f-c514851d2bc8'',''a59236e0-7156-44c1-aab9-44bdaccb9032'',''61c81678-55db-49f8-87c0-675c3d02fd64'',''0e90a085-e2ed-41b5-be10-c2038769428b'',''658ee202-0fc2-4725-81e9-13938c9c7ea0'',''1ac88121-e9bb-4765-93aa-31053d3da19a'',''0242220c-85c5-428b-a3c1-5501d33f304e'',''6a1a13d9-a0e9-408f-bd01-f04837ad46a0'',''28b7536d-fef7-4415-baf9-9068b09161ef'',''bafaf8ff-83fd-49c5-bda6-f9f5775fb651'',''3b53ed03-156b-4814-a6a5-e3b8152b2d6b'',''212030dc-2519-4a2a-a7cf-4085d78d9ac8'',''3723d9e5-0fb5-420c-b8f9-7b2b77d5608e'',''6ca5f41c-75c0-42c6-8a02-98539a4389f4'',''743c15c7-791d-43c6-8150-dc1eb75da929'',''f6a8c896-aa41-465e-b97f-c420a8f893b3'',''29b8ea2b-3a33-4bb5-aefc-55740cd0bb11'',''ec1c041c-9538-4b5f-8e45-b7b15056fe60'',''261bd9d1-2e8b-4ab5-bd79-a3a810981294'',''c11be711-66d6-4f78-8eba-680091ee67f3'',''ec595d3e-4410-4277-8e12-a6bbb93d6f42'',''5fd0d30b-df87-456c-8f4e-fc5be519544a'',''7f7fb69f-7ab2-485f-8a8e-74938b49cd7a'',''271ad2a5-f148-4350-8e8a-56a09e72376e'',''3606953b-41ed-45b8-9eef-493d2506c308'',''670f580c-703a-4f29-a8a6-feea22629225'',''d5324c65-ee58-4bb9-a6da-ebbfc0ab0a5c'',''d0bd6b6f-5d69-47f8-86df-ccd5fdfab270'',''f746ece4-7af8-49ac-8b91-aca111f5a03e'',''1ef3d8f5-81e9-42ba-b81b-f1e93a63404a'',''a01bde2e-374d-4c9f-ac99-1a86b7910e78'',''865f7337-8fea-44dc-8d9a-9e9624afa4bd'',''ed9a6fe8-51a9-427c-bc05-e3e91f7885a4'',''0e7e3fab-3dee-4ea0-877b-fdb1a4a6fadc'',''b18372b9-c4ad-4532-9ef2-267c896390f1'',''38bf123e-c2db-4dad-bc9a-5466e1fd286f'',''9b9c5012-6566-4b57-9b0d-f57182398af2'',''e277405c-c8eb-451e-b913-7f05cc3c483b''',@p747=N'''e277405c-c8eb-451e-b913-7f05cc3c483b'',''9b9c5012-6566-4b57-9b0d-f57182398af2'',''fafa8ef9-de0e-4b6b-b64f-129f806480cc'',''1d2b76d7-01be-438a-b1a5-682b06ba8565'',''ed9a6fe8-51a9-427c-bc05-e3e91f7885a4'',''a01bde2e-374d-4c9f-ac99-1a86b7910e78'',''abab36d3-da4a-4b3f-a085-945ea8ecbebc'',''1ef3d8f5-81e9-42ba-b81b-f1e93a63404a'',''97bba6c7-36f8-4e3a-8ff7-299ed43042fe'',''326de834-f520-4f8c-a531-b1e95bf1b0dd'',''805f9a7f-b46c-4c54-83c1-74c4ead0be16'',''fad080ac-9812-4747-955f-3f698f807a7c'',''32e1fce6-3901-490d-823e-005565470319'',''d0bd6b6f-5d69-47f8-86df-ccd5fdfab270'',''d5324c65-ee58-4bb9-a6da-ebbfc0ab0a5c'',''aa4a7586-b4a4-4d2f-8ca9-6a8617892267'',''670f580c-703a-4f29-a8a6-feea22629225'',''3606953b-41ed-45b8-9eef-493d2506c308'',''7f7fb69f-7ab2-485f-8a8e-74938b49cd7a'',''4b17fdae-cb4d-45f1-a02e-7a3ce5f315f1'',''ec595d3e-4410-4277-8e12-a6bbb93d6f42'',''29b8ea2b-3a33-4bb5-aefc-55740cd0bb11'',''c61282cb-e8e0-464b-9a06-3190d503694d'',''212030dc-2519-4a2a-a7cf-4085d78d9ac8'',''915cb3e5-5c1a-4202-b72a-9b91f02833fd'',''3b53ed03-156b-4814-a6a5-e3b8152b2d6b'',''28b7536d-fef7-4415-baf9-9068b09161ef'',''996e5e41-47e5-4535-b67d-37fb5ef7e48f'',''a16c0e40-0546-4896-98b5-8eb117656c88'',''0242220c-85c5-428b-a3c1-5501d33f304e'',''1ac88121-e9bb-4765-93aa-31053d3da19a'',''e1f9dbc3-e56a-443f-b6ce-b140ab5738eb'',''cd8f6cf6-96dc-4161-a22f-c514851d2bc8'',''9d690b7b-f9cc-4d64-8013-3cf3df47effb'',''06889c07-102b-4dfe-be0a-9b25caffd48d'',''59f114c8-5f9f-4049-ad30-aa1e81667559'',''910e1bff-5065-4de7-a3ac-4a63fdc5904a'',''f60ccaa2-bc54-4eda-9fb4-53ae34c60823'',''ab08248f-6d48-4ade-b11c-cc1f6c369a8f'',''3df7c0ae-e5ec-4dbe-8622-2e916b380fc8'',''de225501-f45d-4cee-9153-f9df6c46dd1f'',''2d946982-6f44-43de-a309-f3e8649ceb5a'',''6bf14d79-4d59-4964-b4f8-8cbf55fad394'',''8d49a266-1467-4d90-b25d-a6dfa74047db'',''159d9b27-4537-4ea1-a632-4b18b4ad632c'',''45748a48-713a-4155-b7db-3b8e87c2782e'',''038b5ab8-4e06-4961-8a67-2493ae2c7d73'',''b87c90a6-a074-4d66-bee8-77ce4ddecd77'',''85dfd1f1-3ee6-42ab-bdaa-7b97c0541655'',''981c7add-9a60-4a8d-ba11-260a4ef627d0'',''7fe0014e-173c-4ef1-9121-c3dbff48e514'',''5ae3930e-4acb-4bb7-80dc-61a4469cee0d'',''c79c760d-39bf-4b0c-bf09-e069ba51359c'',''6721c3fa-bcf5-4839-a444-40ef6492f214'',''252d55f3-e511-4135-bf76-0e6711b63ca1''',@p748='1900-01-01 00:00:00:000',@p749='2079-06-06 23:59:29:000',@p751=NULL,@p752=NULL,@p753=NULL,@p754=NULL,@p755=NULL,@p756=NULL,@p757=NULL,@p758=N'',@p759=N'',@p760=N'',@p761=N'',@p762=N'',@p763=N'',@p765=NULL,@p766=N'',@p767=N'',@p768=N'',@p769=NULL,@p770=NULL,@p771=NULL,@p772=NULL,@p773=NULL,@p774=NULL,@p775='1900-01-01 00:00:00:000',@p776='2079-06-06 23:59:29:000',@p777='1900-01-01 00:00:00:000',@p778='2079-06-06 23:59:29:000',@p779=N'AU',@p780=N'',@p781=N'',@p782=NULL,@p783=NULL,@p784='6721C3FA-BCF5-4839-A444-40EF6492F214',@p785='C79C760D-39BF-4B0C-BF09-E069BA51359C',@p786='5AE3930E-4ACB-4BB7-80DC-61A4469CEE0D',@p787='7FE0014E-173C-4EF1-9121-C3DBFF48E514',@p788='953CE83B-3D2B-476C-B139-42E28ABD3E5E',@p789='038B5AB8-4E06-4961-8A67-2493AE2C7D73',@p790='45748A48-713A-4155-B7DB-3B8E87C2782E',@p791='159D9B27-4537-4EA1-A632-4B18B4AD632C',@p792='B2AE038F-7735-4EDF-8186-4CB59FCC7E3F',@p793='8D49A266-1467-4D90-B25D-A6DFA74047DB',@p794='C80B8CC5-625B-4C2C-BCDF-3B3E3E697D05',@p795='3DF7C0AE-E5EC-4DBE-8622-2E916B380FC8',@p796='FDCCB3A4-A093-445C-814F-FB1C0D1D97D4',@p797='AB08248F-6D48-4ADE-B11C-CC1F6C369A8F',@p798='910E1BFF-5065-4DE7-A3AC-4A63FDC5904A',@p799='59F114C8-5F9F-4049-AD30-AA1E81667559',@p800='06889C07-102B-4DFE-BE0A-9B25CAFFD48D',@p801='9D690B7B-F9CC-4D64-8013-3CF3DF47EFFB',@p802='3C9CD1BA-E697-4D82-B21D-241ADFB1F8D0',@p803='CD8F6CF6-96DC-4161-A22F-C514851D2BC8',@p804='A59236E0-7156-44C1-AAB9-44BDACCB9032',@p805='61C81678-55DB-49F8-87C0-675C3D02FD64',@p806='0E90A085-E2ED-41B5-BE10-C2038769428B',@p807='658EE202-0FC2-4725-81E9-13938C9C7EA0',@p808='1AC88121-E9BB-4765-93AA-31053D3DA19A',@p809='0242220C-85C5-428B-A3C1-5501D33F304E',@p810='6A1A13D9-A0E9-408F-BD01-F04837AD46A0',@p811='28B7536D-FEF7-4415-BAF9-9068B09161EF',@p812='BAFAF8FF-83FD-49C5-BDA6-F9F5775FB651',@p813='3B53ED03-156B-4814-A6A5-E3B8152B2D6B',@p814='212030DC-2519-4A2A-A7CF-4085D78D9AC8',@p815='3723D9E5-0FB5-420C-B8F9-7B2B77D5608E',@p816='6CA5F41C-75C0-42C6-8A02-98539A4389F4',@p817='743C15C7-791D-43C6-8150-DC1EB75DA929',@p818='F6A8C896-AA41-465E-B97F-C420A8F893B3',@p819='29B8EA2B-3A33-4BB5-AEFC-55740CD0BB11',@p820='EC1C041C-9538-4B5F-8E45-B7B15056FE60',@p821='261BD9D1-2E8B-4AB5-BD79-A3A810981294',@p822='C11BE711-66D6-4F78-8EBA-680091EE67F3',@p823='EC595D3E-4410-4277-8E12-A6BBB93D6F42',@p824='5FD0D30B-DF87-456C-8F4E-FC5BE519544A',@p825='7F7FB69F-7AB2-485F-8A8E-74938B49CD7A',@p826='271AD2A5-F148-4350-8E8A-56A09E72376E',@p827='3606953B-41ED-45B8-9EEF-493D2506C308',@p828='670F580C-703A-4F29-A8A6-FEEA22629225',@p829='D5324C65-EE58-4BB9-A6DA-EBBFC0AB0A5C',@p830='D0BD6B6F-5D69-47F8-86DF-CCD5FDFAB270',@p831='F746ECE4-7AF8-49AC-8B91-ACA111F5A03E',@p832='1EF3D8F5-81E9-42BA-B81B-F1E93A63404A',@p833='A01BDE2E-374D-4C9F-AC99-1A86B7910E78',@p834='865F7337-8FEA-44DC-8D9A-9E9624AFA4BD',@p835='ED9A6FE8-51A9-427C-BC05-E3E91F7885A4',@p836='0E7E3FAB-3DEE-4EA0-877B-FDB1A4A6FADC',@p837='B18372B9-C4AD-4532-9EF2-267C896390F1',@p838='38BF123E-C2DB-4DAD-BC9A-5466E1FD286F',@p839='9B9C5012-6566-4B57-9B0D-F57182398AF2',@p840='E277405C-C8EB-451E-B913-7F05CC3C483B',@p841='E277405C-C8EB-451E-B913-7F05CC3C483B',@p842='9B9C5012-6566-4B57-9B0D-F57182398AF2',@p843='FAFA8EF9-DE0E-4B6B-B64F-129F806480CC',@p844='1D2B76D7-01BE-438A-B1A5-682B06BA8565',@p845='ED9A6FE8-51A9-427C-BC05-E3E91F7885A4',@p846='A01BDE2E-374D-4C9F-AC99-1A86B7910E78',@p847='ABAB36D3-DA4A-4B3F-A085-945EA8ECBEBC',@p848='1EF3D8F5-81E9-42BA-B81B-F1E93A63404A',@p849='97BBA6C7-36F8-4E3A-8FF7-299ED43042FE',@p850='326DE834-F520-4F8C-A531-B1E95BF1B0DD',@p851='805F9A7F-B46C-4C54-83C1-74C4EAD0BE16',@p852='FAD080AC-9812-4747-955F-3F698F807A7C',@p853='32E1FCE6-3901-490D-823E-005565470319',@p854='D0BD6B6F-5D69-47F8-86DF-CCD5FDFAB270',@p855='D5324C65-EE58-4BB9-A6DA-EBBFC0AB0A5C',@p856='AA4A7586-B4A4-4D2F-8CA9-6A8617892267',@p857='670F580C-703A-4F29-A8A6-FEEA22629225',@p858='3606953B-41ED-45B8-9EEF-493D2506C308',@p859='7F7FB69F-7AB2-485F-8A8E-74938B49CD7A',@p860='4B17FDAE-CB4D-45F1-A02E-7A3CE5F315F1',@p861='EC595D3E-4410-4277-8E12-A6BBB93D6F42',@p862='29B8EA2B-3A33-4BB5-AEFC-55740CD0BB11',@p863='C61282CB-E8E0-464B-9A06-3190D503694D',@p864='212030DC-2519-4A2A-A7CF-4085D78D9AC8',@p865='915CB3E5-5C1A-4202-B72A-9B91F02833FD',@p866='3B53ED03-156B-4814-A6A5-E3B8152B2D6B',@p867='28B7536D-FEF7-4415-BAF9-9068B09161EF',@p868='996E5E41-47E5-4535-B67D-37FB5EF7E48F',@p869='A16C0E40-0546-4896-98B5-8EB117656C88',@p870='0242220C-85C5-428B-A3C1-5501D33F304E',@p871='1AC88121-E9BB-4765-93AA-31053D3DA19A',@p872='E1F9DBC3-E56A-443F-B6CE-B140AB5738EB',@p873='CD8F6CF6-96DC-4161-A22F-C514851D2BC8',@p874='9D690B7B-F9CC-4D64-8013-3CF3DF47EFFB',@p875='06889C07-102B-4DFE-BE0A-9B25CAFFD48D',@p876='59F114C8-5F9F-4049-AD30-AA1E81667559',@p877='910E1BFF-5065-4DE7-A3AC-4A63FDC5904A',@p878='F60CCAA2-BC54-4EDA-9FB4-53AE34C60823',@p879='AB08248F-6D48-4ADE-B11C-CC1F6C369A8F',@p880='3DF7C0AE-E5EC-4DBE-8622-2E916B380FC8',@p881='DE225501-F45D-4CEE-9153-F9DF6C46DD1F',@p882='2D946982-6F44-43DE-A309-F3E8649CEB5A',@p883='6BF14D79-4D59-4964-B4F8-8CBF55FAD394',@p884='8D49A266-1467-4D90-B25D-A6DFA74047DB',@p885='159D9B27-4537-4EA1-A632-4B18B4AD632C',@p886='45748A48-713A-4155-B7DB-3B8E87C2782E',@p887='038B5AB8-4E06-4961-8A67-2493AE2C7D73',@p888='B87C90A6-A074-4D66-BEE8-77CE4DDECD77',@p889='85DFD1F1-3EE6-42AB-BDAA-7B97C0541655',@p890='981C7ADD-9A60-4A8D-BA11-260A4EF627D0',@p891='7FE0014E-173C-4EF1-9121-C3DBFF48E514',@p892='5AE3930E-4ACB-4BB7-80DC-61A4469CEE0D',@p893='C79C760D-39BF-4B0C-BF09-E069BA51359C',@p894='6721C3FA-BCF5-4839-A444-40EF6492F214',@p895='252D55F3-E511-4135-BF76-0E6711B63CA1', @p896=N'', @p897=N''");
			AssertEquals("Result should have rows", 0, result.Rows.Count);
		}

		public void TestSummaryByJobShouldHaveAdditionalColumns()
		{
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "exec sp_executesql N'EXEC Report_JobProfit @IsSummaryByJob = ''Y'', @IncludeJobDetails = ''Y'', @JobType = @p727, @GroupByJob = ''Y'', @JH_GC = @p728, @JH_Status = @p729, @JH_BranchPKList = @p730,  @JH_departmentpkList = @p731 , @JH_SalesRepPK = @p732, @JH_OperatorPK = @p733 , @JH_FromCreatedDate = @p734 , @JH_ToCreatedDate = @p735, @JH_FromClosedDate = @p736 , @JH_ToClosedDate = @p737, @JH_FromRevenueRecognizedDate = @p738 , @JH_ToRevenueRecognizedDate = @p739, @JH_LocalClientPKList = @p740, @JH_OverseasAgentPKList =  @p741, @AC_ChargeGroup =  @p742 , @AL_BranchPKList = @p743, @AL_departmentpkList = @p744, @AL_ChargeCodePKList = @p745, @AL_CreditorPKList = @p746, @AL_DebtorPKList = @p747, @AL_FromDate = @p748, @AL_ToDate = @p749, @AL_ExcludeReversedWIPACR = @p762, @JobShipmentList = @p751, @JobDeclarationList = @p751, @JS_TransportMode =@p752 , @JE_TransportMode =@p753, @JS_ContainerMode = @p754, @JE_ContainerMode = @p755, @FW_OriginPK =@p756 , @FW_DestinationPK = @p757, @FW_FromETA = @p758, @FW_ToETA = @p759, @FW_FromETD = @p760, @FW_ToETD = @p761, @AL_OutstandingWIPOnly = @p762, @AL_OutstandingACROnly = @p763, @JobConsolList = @p765, @JK_ContainerMode =@p766  , @JK_TransportMode =@p767 , @JK_AgentType =@p768 , @JK_SendingAgentPK =@p769 , @JK_ReceivingAgentPK =@p770 , @JK_ColoadAgentPK =@p771 , @JK_CarrierPK =@p772 , @JK_JX_LoadPort =@p773 , @JK_JX_DischargePort =@p774 , @JK_JX_FromETD =@p775 , @JK_JX_ToETD =@p776 , @JK_JX_FromETA =@p777 , @JK_JX_ToETA =@p778 , @CurrentCountry = @p779, @FW_FromRegistration = @p780, @FW_ToRegistration = @p781, @AC_AR_SalesGroup = @p782, @AC_AR_ExpenseGroup = @p783, @Gateway = @p896',N'@p727 nvarchar(4000),@p728 uniqueidentifier,@p729 nvarchar(4000),@p730 nvarchar(4000),@p731 nvarchar(4000),@p732 nvarchar(4000),@p733 nvarchar(4000),@p734 nvarchar(4000),@p735 nvarchar(4000),@p736 nvarchar(4000),@p737 nvarchar(4000),@p738 nvarchar(4000),@p739 nvarchar(4000),@p740 nvarchar(4000),@p741 nvarchar(4000),@p742 nvarchar(4000),@p743 nvarchar(4000),@p744 nvarchar(4000),@p745 nvarchar(4000),@p746 nvarchar(2222),@p747 nvarchar(2144),@p748 datetime,@p749 datetime,@p751 nvarchar(4000),@p752 nvarchar(4000),@p753 nvarchar(4000),@p754 nvarchar(4000),@p755 nvarchar(4000),@p756 nvarchar(4000),@p757 nvarchar(4000),@p758 nvarchar(4000),@p759 nvarchar(4000),@p760 nvarchar(4000),@p761 nvarchar(4000),@p762 nvarchar(4000),@p763 nvarchar(4000),@p765 nvarchar(4000),@p766 nvarchar(4000),@p767 nvarchar(4000),@p768 nvarchar(4000),@p769 nvarchar(4000),@p770 nvarchar(4000),@p771 nvarchar(4000),@p772 nvarchar(4000),@p773 nvarchar(4000),@p774 nvarchar(4000),@p775 datetime,@p776 datetime,@p777 datetime,@p778 datetime,@p779 nvarchar(2),@p780 nvarchar(4000),@p781 nvarchar(4000),@p782 nvarchar(4000),@p783 nvarchar(4000),@p784 uniqueidentifier,@p785 uniqueidentifier,@p786 uniqueidentifier,@p787 uniqueidentifier,@p788 uniqueidentifier,@p789 uniqueidentifier,@p790 uniqueidentifier,@p791 uniqueidentifier,@p792 uniqueidentifier,@p793 uniqueidentifier,@p794 uniqueidentifier,@p795 uniqueidentifier,@p796 uniqueidentifier,@p797 uniqueidentifier,@p798 uniqueidentifier,@p799 uniqueidentifier,@p800 uniqueidentifier,@p801 uniqueidentifier,@p802 uniqueidentifier,@p803 uniqueidentifier,@p804 uniqueidentifier,@p805 uniqueidentifier,@p806 uniqueidentifier,@p807 uniqueidentifier,@p808 uniqueidentifier,@p809 uniqueidentifier,@p810 uniqueidentifier,@p811 uniqueidentifier,@p812 uniqueidentifier,@p813 uniqueidentifier,@p814 uniqueidentifier,@p815 uniqueidentifier,@p816 uniqueidentifier,@p817 uniqueidentifier,@p818 uniqueidentifier,@p819 uniqueidentifier,@p820 uniqueidentifier,@p821 uniqueidentifier,@p822 uniqueidentifier,@p823 uniqueidentifier,@p824 uniqueidentifier,@p825 uniqueidentifier,@p826 uniqueidentifier,@p827 uniqueidentifier,@p828 uniqueidentifier,@p829 uniqueidentifier,@p830 uniqueidentifier,@p831 uniqueidentifier,@p832 uniqueidentifier,@p833 uniqueidentifier,@p834 uniqueidentifier,@p835 uniqueidentifier,@p836 uniqueidentifier,@p837 uniqueidentifier,@p838 uniqueidentifier,@p839 uniqueidentifier,@p840 uniqueidentifier,@p841 uniqueidentifier,@p842 uniqueidentifier,@p843 uniqueidentifier,@p844 uniqueidentifier,@p845 uniqueidentifier,@p846 uniqueidentifier,@p847 uniqueidentifier,@p848 uniqueidentifier,@p849 uniqueidentifier,@p850 uniqueidentifier,@p851 uniqueidentifier,@p852 uniqueidentifier,@p853 uniqueidentifier,@p854 uniqueidentifier,@p855 uniqueidentifier,@p856 uniqueidentifier,@p857 uniqueidentifier,@p858 uniqueidentifier,@p859 uniqueidentifier,@p860 uniqueidentifier,@p861 uniqueidentifier,@p862 uniqueidentifier,@p863 uniqueidentifier,@p864 uniqueidentifier,@p865 uniqueidentifier,@p866 uniqueidentifier,@p867 uniqueidentifier,@p868 uniqueidentifier,@p869 uniqueidentifier,@p870 uniqueidentifier,@p871 uniqueidentifier,@p872 uniqueidentifier,@p873 uniqueidentifier,@p874 uniqueidentifier,@p875 uniqueidentifier,@p876 uniqueidentifier,@p877 uniqueidentifier,@p878 uniqueidentifier,@p879 uniqueidentifier,@p880 uniqueidentifier,@p881 uniqueidentifier,@p882 uniqueidentifier,@p883 uniqueidentifier,@p884 uniqueidentifier,@p885 uniqueidentifier,@p886 uniqueidentifier,@p887 uniqueidentifier,@p888 uniqueidentifier,@p889 uniqueidentifier,@p890 uniqueidentifier,@p891 uniqueidentifier,@p892 uniqueidentifier,@p893 uniqueidentifier,@p894 uniqueidentifier,@p895 uniqueidentifier, @p896 nvarchar(3)', @p727=N'',@p728='B651EAE1-D86C-4715-B0E5-A748DEF329DF',@p729=N'',@p730=N'',@p731=N'',@p732=NULL,@p733=NULL,@p734=N'',@p735=N'',@p736=N'',@p737=N'',@p738=N'',@p739=N'',@p740=N'',@p741=N'',@p742=N'',@p743=N'',@p744=N'',@p745=N'',@p746=N'''6721c3fa-bcf5-4839-a444-40ef6492f214'',''c79c760d-39bf-4b0c-bf09-e069ba51359c'',''5ae3930e-4acb-4bb7-80dc-61a4469cee0d'',''7fe0014e-173c-4ef1-9121-c3dbff48e514'',''953ce83b-3d2b-476c-b139-42e28abd3e5e'',''038b5ab8-4e06-4961-8a67-2493ae2c7d73'',''45748a48-713a-4155-b7db-3b8e87c2782e'',''159d9b27-4537-4ea1-a632-4b18b4ad632c'',''b2ae038f-7735-4edf-8186-4cb59fcc7e3f'',''8d49a266-1467-4d90-b25d-a6dfa74047db'',''c80b8cc5-625b-4c2c-bcdf-3b3e3e697d05'',''3df7c0ae-e5ec-4dbe-8622-2e916b380fc8'',''fdccb3a4-a093-445c-814f-fb1c0d1d97d4'',''ab08248f-6d48-4ade-b11c-cc1f6c369a8f'',''910e1bff-5065-4de7-a3ac-4a63fdc5904a'',''59f114c8-5f9f-4049-ad30-aa1e81667559'',''06889c07-102b-4dfe-be0a-9b25caffd48d'',''9d690b7b-f9cc-4d64-8013-3cf3df47effb'',''3c9cd1ba-e697-4d82-b21d-241adfb1f8d0'',''cd8f6cf6-96dc-4161-a22f-c514851d2bc8'',''a59236e0-7156-44c1-aab9-44bdaccb9032'',''61c81678-55db-49f8-87c0-675c3d02fd64'',''0e90a085-e2ed-41b5-be10-c2038769428b'',''658ee202-0fc2-4725-81e9-13938c9c7ea0'',''1ac88121-e9bb-4765-93aa-31053d3da19a'',''0242220c-85c5-428b-a3c1-5501d33f304e'',''6a1a13d9-a0e9-408f-bd01-f04837ad46a0'',''28b7536d-fef7-4415-baf9-9068b09161ef'',''bafaf8ff-83fd-49c5-bda6-f9f5775fb651'',''3b53ed03-156b-4814-a6a5-e3b8152b2d6b'',''212030dc-2519-4a2a-a7cf-4085d78d9ac8'',''3723d9e5-0fb5-420c-b8f9-7b2b77d5608e'',''6ca5f41c-75c0-42c6-8a02-98539a4389f4'',''743c15c7-791d-43c6-8150-dc1eb75da929'',''f6a8c896-aa41-465e-b97f-c420a8f893b3'',''29b8ea2b-3a33-4bb5-aefc-55740cd0bb11'',''ec1c041c-9538-4b5f-8e45-b7b15056fe60'',''261bd9d1-2e8b-4ab5-bd79-a3a810981294'',''c11be711-66d6-4f78-8eba-680091ee67f3'',''ec595d3e-4410-4277-8e12-a6bbb93d6f42'',''5fd0d30b-df87-456c-8f4e-fc5be519544a'',''7f7fb69f-7ab2-485f-8a8e-74938b49cd7a'',''271ad2a5-f148-4350-8e8a-56a09e72376e'',''3606953b-41ed-45b8-9eef-493d2506c308'',''670f580c-703a-4f29-a8a6-feea22629225'',''d5324c65-ee58-4bb9-a6da-ebbfc0ab0a5c'',''d0bd6b6f-5d69-47f8-86df-ccd5fdfab270'',''f746ece4-7af8-49ac-8b91-aca111f5a03e'',''1ef3d8f5-81e9-42ba-b81b-f1e93a63404a'',''a01bde2e-374d-4c9f-ac99-1a86b7910e78'',''865f7337-8fea-44dc-8d9a-9e9624afa4bd'',''ed9a6fe8-51a9-427c-bc05-e3e91f7885a4'',''0e7e3fab-3dee-4ea0-877b-fdb1a4a6fadc'',''b18372b9-c4ad-4532-9ef2-267c896390f1'',''38bf123e-c2db-4dad-bc9a-5466e1fd286f'',''9b9c5012-6566-4b57-9b0d-f57182398af2'',''e277405c-c8eb-451e-b913-7f05cc3c483b''',@p747=N'''e277405c-c8eb-451e-b913-7f05cc3c483b'',''9b9c5012-6566-4b57-9b0d-f57182398af2'',''fafa8ef9-de0e-4b6b-b64f-129f806480cc'',''1d2b76d7-01be-438a-b1a5-682b06ba8565'',''ed9a6fe8-51a9-427c-bc05-e3e91f7885a4'',''a01bde2e-374d-4c9f-ac99-1a86b7910e78'',''abab36d3-da4a-4b3f-a085-945ea8ecbebc'',''1ef3d8f5-81e9-42ba-b81b-f1e93a63404a'',''97bba6c7-36f8-4e3a-8ff7-299ed43042fe'',''326de834-f520-4f8c-a531-b1e95bf1b0dd'',''805f9a7f-b46c-4c54-83c1-74c4ead0be16'',''fad080ac-9812-4747-955f-3f698f807a7c'',''32e1fce6-3901-490d-823e-005565470319'',''d0bd6b6f-5d69-47f8-86df-ccd5fdfab270'',''d5324c65-ee58-4bb9-a6da-ebbfc0ab0a5c'',''aa4a7586-b4a4-4d2f-8ca9-6a8617892267'',''670f580c-703a-4f29-a8a6-feea22629225'',''3606953b-41ed-45b8-9eef-493d2506c308'',''7f7fb69f-7ab2-485f-8a8e-74938b49cd7a'',''4b17fdae-cb4d-45f1-a02e-7a3ce5f315f1'',''ec595d3e-4410-4277-8e12-a6bbb93d6f42'',''29b8ea2b-3a33-4bb5-aefc-55740cd0bb11'',''c61282cb-e8e0-464b-9a06-3190d503694d'',''212030dc-2519-4a2a-a7cf-4085d78d9ac8'',''915cb3e5-5c1a-4202-b72a-9b91f02833fd'',''3b53ed03-156b-4814-a6a5-e3b8152b2d6b'',''28b7536d-fef7-4415-baf9-9068b09161ef'',''996e5e41-47e5-4535-b67d-37fb5ef7e48f'',''a16c0e40-0546-4896-98b5-8eb117656c88'',''0242220c-85c5-428b-a3c1-5501d33f304e'',''1ac88121-e9bb-4765-93aa-31053d3da19a'',''e1f9dbc3-e56a-443f-b6ce-b140ab5738eb'',''cd8f6cf6-96dc-4161-a22f-c514851d2bc8'',''9d690b7b-f9cc-4d64-8013-3cf3df47effb'',''06889c07-102b-4dfe-be0a-9b25caffd48d'',''59f114c8-5f9f-4049-ad30-aa1e81667559'',''910e1bff-5065-4de7-a3ac-4a63fdc5904a'',''f60ccaa2-bc54-4eda-9fb4-53ae34c60823'',''ab08248f-6d48-4ade-b11c-cc1f6c369a8f'',''3df7c0ae-e5ec-4dbe-8622-2e916b380fc8'',''de225501-f45d-4cee-9153-f9df6c46dd1f'',''2d946982-6f44-43de-a309-f3e8649ceb5a'',''6bf14d79-4d59-4964-b4f8-8cbf55fad394'',''8d49a266-1467-4d90-b25d-a6dfa74047db'',''159d9b27-4537-4ea1-a632-4b18b4ad632c'',''45748a48-713a-4155-b7db-3b8e87c2782e'',''038b5ab8-4e06-4961-8a67-2493ae2c7d73'',''b87c90a6-a074-4d66-bee8-77ce4ddecd77'',''85dfd1f1-3ee6-42ab-bdaa-7b97c0541655'',''981c7add-9a60-4a8d-ba11-260a4ef627d0'',''7fe0014e-173c-4ef1-9121-c3dbff48e514'',''5ae3930e-4acb-4bb7-80dc-61a4469cee0d'',''c79c760d-39bf-4b0c-bf09-e069ba51359c'',''6721c3fa-bcf5-4839-a444-40ef6492f214'',''252d55f3-e511-4135-bf76-0e6711b63ca1''',@p748='1900-01-01 00:00:00:000',@p749='2079-06-06 23:59:29:000',@p751=NULL,@p752=NULL,@p753=NULL,@p754=NULL,@p755=NULL,@p756=NULL,@p757=NULL,@p758=N'',@p759=N'',@p760=N'',@p761=N'',@p762=N'',@p763=N'',@p765=NULL,@p766=N'',@p767=N'',@p768=N'',@p769=NULL,@p770=NULL,@p771=NULL,@p772=NULL,@p773=NULL,@p774=NULL,@p775='1900-01-01 00:00:00:000',@p776='2079-06-06 23:59:29:000',@p777='1900-01-01 00:00:00:000',@p778='2079-06-06 23:59:29:000',@p779=N'AU',@p780=N'',@p781=N'',@p782=NULL,@p783=NULL,@p784='6721C3FA-BCF5-4839-A444-40EF6492F214',@p785='C79C760D-39BF-4B0C-BF09-E069BA51359C',@p786='5AE3930E-4ACB-4BB7-80DC-61A4469CEE0D',@p787='7FE0014E-173C-4EF1-9121-C3DBFF48E514',@p788='953CE83B-3D2B-476C-B139-42E28ABD3E5E',@p789='038B5AB8-4E06-4961-8A67-2493AE2C7D73',@p790='45748A48-713A-4155-B7DB-3B8E87C2782E',@p791='159D9B27-4537-4EA1-A632-4B18B4AD632C',@p792='B2AE038F-7735-4EDF-8186-4CB59FCC7E3F',@p793='8D49A266-1467-4D90-B25D-A6DFA74047DB',@p794='C80B8CC5-625B-4C2C-BCDF-3B3E3E697D05',@p795='3DF7C0AE-E5EC-4DBE-8622-2E916B380FC8',@p796='FDCCB3A4-A093-445C-814F-FB1C0D1D97D4',@p797='AB08248F-6D48-4ADE-B11C-CC1F6C369A8F',@p798='910E1BFF-5065-4DE7-A3AC-4A63FDC5904A',@p799='59F114C8-5F9F-4049-AD30-AA1E81667559',@p800='06889C07-102B-4DFE-BE0A-9B25CAFFD48D',@p801='9D690B7B-F9CC-4D64-8013-3CF3DF47EFFB',@p802='3C9CD1BA-E697-4D82-B21D-241ADFB1F8D0',@p803='CD8F6CF6-96DC-4161-A22F-C514851D2BC8',@p804='A59236E0-7156-44C1-AAB9-44BDACCB9032',@p805='61C81678-55DB-49F8-87C0-675C3D02FD64',@p806='0E90A085-E2ED-41B5-BE10-C2038769428B',@p807='658EE202-0FC2-4725-81E9-13938C9C7EA0',@p808='1AC88121-E9BB-4765-93AA-31053D3DA19A',@p809='0242220C-85C5-428B-A3C1-5501D33F304E',@p810='6A1A13D9-A0E9-408F-BD01-F04837AD46A0',@p811='28B7536D-FEF7-4415-BAF9-9068B09161EF',@p812='BAFAF8FF-83FD-49C5-BDA6-F9F5775FB651',@p813='3B53ED03-156B-4814-A6A5-E3B8152B2D6B',@p814='212030DC-2519-4A2A-A7CF-4085D78D9AC8',@p815='3723D9E5-0FB5-420C-B8F9-7B2B77D5608E',@p816='6CA5F41C-75C0-42C6-8A02-98539A4389F4',@p817='743C15C7-791D-43C6-8150-DC1EB75DA929',@p818='F6A8C896-AA41-465E-B97F-C420A8F893B3',@p819='29B8EA2B-3A33-4BB5-AEFC-55740CD0BB11',@p820='EC1C041C-9538-4B5F-8E45-B7B15056FE60',@p821='261BD9D1-2E8B-4AB5-BD79-A3A810981294',@p822='C11BE711-66D6-4F78-8EBA-680091EE67F3',@p823='EC595D3E-4410-4277-8E12-A6BBB93D6F42',@p824='5FD0D30B-DF87-456C-8F4E-FC5BE519544A',@p825='7F7FB69F-7AB2-485F-8A8E-74938B49CD7A',@p826='271AD2A5-F148-4350-8E8A-56A09E72376E',@p827='3606953B-41ED-45B8-9EEF-493D2506C308',@p828='670F580C-703A-4F29-A8A6-FEEA22629225',@p829='D5324C65-EE58-4BB9-A6DA-EBBFC0AB0A5C',@p830='D0BD6B6F-5D69-47F8-86DF-CCD5FDFAB270',@p831='F746ECE4-7AF8-49AC-8B91-ACA111F5A03E',@p832='1EF3D8F5-81E9-42BA-B81B-F1E93A63404A',@p833='A01BDE2E-374D-4C9F-AC99-1A86B7910E78',@p834='865F7337-8FEA-44DC-8D9A-9E9624AFA4BD',@p835='ED9A6FE8-51A9-427C-BC05-E3E91F7885A4',@p836='0E7E3FAB-3DEE-4EA0-877B-FDB1A4A6FADC',@p837='B18372B9-C4AD-4532-9EF2-267C896390F1',@p838='38BF123E-C2DB-4DAD-BC9A-5466E1FD286F',@p839='9B9C5012-6566-4B57-9B0D-F57182398AF2',@p840='E277405C-C8EB-451E-B913-7F05CC3C483B',@p841='E277405C-C8EB-451E-B913-7F05CC3C483B',@p842='9B9C5012-6566-4B57-9B0D-F57182398AF2',@p843='FAFA8EF9-DE0E-4B6B-B64F-129F806480CC',@p844='1D2B76D7-01BE-438A-B1A5-682B06BA8565',@p845='ED9A6FE8-51A9-427C-BC05-E3E91F7885A4',@p846='A01BDE2E-374D-4C9F-AC99-1A86B7910E78',@p847='ABAB36D3-DA4A-4B3F-A085-945EA8ECBEBC',@p848='1EF3D8F5-81E9-42BA-B81B-F1E93A63404A',@p849='97BBA6C7-36F8-4E3A-8FF7-299ED43042FE',@p850='326DE834-F520-4F8C-A531-B1E95BF1B0DD',@p851='805F9A7F-B46C-4C54-83C1-74C4EAD0BE16',@p852='FAD080AC-9812-4747-955F-3F698F807A7C',@p853='32E1FCE6-3901-490D-823E-005565470319',@p854='D0BD6B6F-5D69-47F8-86DF-CCD5FDFAB270',@p855='D5324C65-EE58-4BB9-A6DA-EBBFC0AB0A5C',@p856='AA4A7586-B4A4-4D2F-8CA9-6A8617892267',@p857='670F580C-703A-4F29-A8A6-FEEA22629225',@p858='3606953B-41ED-45B8-9EEF-493D2506C308',@p859='7F7FB69F-7AB2-485F-8A8E-74938B49CD7A',@p860='4B17FDAE-CB4D-45F1-A02E-7A3CE5F315F1',@p861='EC595D3E-4410-4277-8E12-A6BBB93D6F42',@p862='29B8EA2B-3A33-4BB5-AEFC-55740CD0BB11',@p863='C61282CB-E8E0-464B-9A06-3190D503694D',@p864='212030DC-2519-4A2A-A7CF-4085D78D9AC8',@p865='915CB3E5-5C1A-4202-B72A-9B91F02833FD',@p866='3B53ED03-156B-4814-A6A5-E3B8152B2D6B',@p867='28B7536D-FEF7-4415-BAF9-9068B09161EF',@p868='996E5E41-47E5-4535-B67D-37FB5EF7E48F',@p869='A16C0E40-0546-4896-98B5-8EB117656C88',@p870='0242220C-85C5-428B-A3C1-5501D33F304E',@p871='1AC88121-E9BB-4765-93AA-31053D3DA19A',@p872='E1F9DBC3-E56A-443F-B6CE-B140AB5738EB',@p873='CD8F6CF6-96DC-4161-A22F-C514851D2BC8',@p874='9D690B7B-F9CC-4D64-8013-3CF3DF47EFFB',@p875='06889C07-102B-4DFE-BE0A-9B25CAFFD48D',@p876='59F114C8-5F9F-4049-AD30-AA1E81667559',@p877='910E1BFF-5065-4DE7-A3AC-4A63FDC5904A',@p878='F60CCAA2-BC54-4EDA-9FB4-53AE34C60823',@p879='AB08248F-6D48-4ADE-B11C-CC1F6C369A8F',@p880='3DF7C0AE-E5EC-4DBE-8622-2E916B380FC8',@p881='DE225501-F45D-4CEE-9153-F9DF6C46DD1F',@p882='2D946982-6F44-43DE-A309-F3E8649CEB5A',@p883='6BF14D79-4D59-4964-B4F8-8CBF55FAD394',@p884='8D49A266-1467-4D90-B25D-A6DFA74047DB',@p885='159D9B27-4537-4EA1-A632-4B18B4AD632C',@p886='45748A48-713A-4155-B7DB-3B8E87C2782E',@p887='038B5AB8-4E06-4961-8A67-2493AE2C7D73',@p888='B87C90A6-A074-4D66-BEE8-77CE4DDECD77',@p889='85DFD1F1-3EE6-42AB-BDAA-7B97C0541655',@p890='981C7ADD-9A60-4A8D-BA11-260A4EF627D0',@p891='7FE0014E-173C-4EF1-9121-C3DBFF48E514',@p892='5AE3930E-4ACB-4BB7-80DC-61A4469CEE0D',@p893='C79C760D-39BF-4B0C-BF09-E069BA51359C',@p894='6721C3FA-BCF5-4839-A444-40EF6492F214',@p895='252D55F3-E511-4135-BF76-0E6711B63CA1', @p896=N''");
			AssertEquals("Result should have rows", 0, result.Rows.Count);

			Assert("Has FW_FCLContainerTEU column", result.Columns.Contains("FW_FCLContainerTEU"));
			Assert("Has FW_ConsolLastDischarge column", result.Columns.Contains("FW_ConsolLastDischarge"));
			Assert("Has FW_ConsolFirstLoad column", result.Columns.Contains("FW_ConsolFirstLoad"));
			Assert("Has FW_ConsolATA column", result.Columns.Contains("FW_ConsolATA"));
			Assert("Has FW_ConsolATD column", result.Columns.Contains("FW_ConsolATD"));
			Assert("Has FW_ConsolETA column", result.Columns.Contains("FW_ConsolETA"));
			Assert("Has FW_ConsolETD column", result.Columns.Contains("FW_ConsolETD"));
		}

		public void TestOutstandingWIPOnlyAndACROnlyFiltersWithWIPACR()
		{
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 117, 100, true);
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 115, 113, true, "2012-07-13", null);
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 30, 20, true, "2012-06-13", "2012-07-13");
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 55, 5, false, "2012-07-13", "2012-07-13");
			CreateJobChargeAndLines(commonChargeCode2, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedDebtorOrgPK, 21, 31, true, "2012-07-13", null);

			var dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", false, false);
			AssertEquals("No WIP or ACR filtering sanity check.", 12, dv.Count);

			dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", true, false);
			AssertEquals("Expect no filtering at line level, all 12 lines should be reported as there is some outstanding WIP in the set.", 12, dv.Count);

			dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", false, true);
			AssertEquals("Expect no filtering at line level, all 12 lines should be reported as there is some outstanding WIP in the set.", 12, dv.Count);

			dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", true, true);
			AssertEquals("Expect no filtering at line level, all 12 lines should be reported as there is some outstanding WIP in the set.", 12, dv.Count);
		}

		public void TestOutstandingWIPOnlyAndACROnlyFiltersWithNoWIPACR()
		{
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 30, 20, false, "2012-07-13", "2012-07-13");

			var dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", false, false);
			AssertEquals("No WIP or ACR filtering sanity check.", 2, dv.Count);

			dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", true, false);
			AssertEquals("Expect no filtering at line level, both lines should not be reported as there is no outstanding WIP in the set.", 0, dv.Count);

			dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", false, true);
			AssertEquals("Expect no filtering at line level, both lines should not be reported as there is no outstanding WIP in the set.", 0, dv.Count);

			dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", true, true);
			AssertEquals("Expect no filtering at line level, both lines should not be reported as there is no outstanding WIP in the set.", 0, dv.Count);
		}

		public void TestIsActiveFiltersWithActiveJob()
		{
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 30, 20, false, "2012-07-13", "2012-07-13");

			var dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", false, false, false, null, null, false, false, "Active");
			AssertEquals("Expect generate job report with active jobs.", 2, dv.Count);

			RunSQL(new object[] { jobPK }, "UPDATE dbo.JobHeader SET JH_IsActive = 0, JH_SystemLastEditTimeUtc = GETUTCDATE(), JH_SystemLastEditUser = '~BP' WHERE JH_PK = {0}");

			dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", false, false, false, null, null, false, false, "Inactive");
			AssertEquals("Expect generate job report with inactive jobs.", 2, dv.Count);

			dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", false, false, false, null, null, false, false, "All");
			AssertEquals("Expect generate job report with all jobs.", 2, dv.Count);
		}

		/// <summary>
		/// Represents settings on the stored procedure which we know will make the SP go down a particular code path in
		/// generating the SQL.
		/// </summary>
		public class CodePathCombination
		{
			public bool IncludeConsolDetails { get; set; }
			public string ConsolFromETD { get; set; }
			public string JobType { get; set; }

			public override string ToString()
			{
				return string.Format("IncludeConsolDetails={0}, ConsolFromETD={1}, JobType={2}", IncludeConsolDetails, ConsolFromETD ?? "null", JobType ?? "null");
			}
		}

		public void TestProfitSummarisedByChargeCodeCorrectlyJobTypeA()
		{
			TestProfitSummarisedByChargeCodeCorrectly(new CodePathCombination { IncludeConsolDetails = false, ConsolFromETD = null, JobType = null });
		}

		public void TestProfitSummarisedByChargeCodeCorrectlyJobTypeS()
		{
			TestProfitSummarisedByChargeCodeCorrectly(new CodePathCombination { IncludeConsolDetails = false, ConsolFromETD = null, JobType = "S" });
		}

		protected void TestProfitSummarisedByChargeCodeCorrectly(CodePathCombination combination)
		{
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 117, 100, true);
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 115, 113, true, "2012-07-13", null);
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 30, 20, true, "2012-06-13", "2012-07-13");
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 55, 5, false, "2012-07-13", "2012-07-13");
			CreateJobChargeAndLines(commonChargeCode2, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedDebtorOrgPK, 21, 31, true, "2012-07-13", null);

			var dv = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", false, false, combination.IncludeConsolDetails, combination.ConsolFromETD, combination.JobType);
			AssertEquals("2 lines created in Setup(), 10 lines in method. Expect 12 in total. Combination = " + combination.ToString(), 12, dv.Count);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 1. Combination = " + combination.ToString(), DBNull.Value, dv[0]["JH_Profit"]);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 2. Combination = " + combination.ToString(), DBNull.Value, dv[1]["JH_Profit"]);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 3. Combination = " + combination.ToString(), DBNull.Value, dv[2]["JH_Profit"]);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 4. Combination = " + combination.ToString(), DBNull.Value, dv[3]["JH_Profit"]);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 5. Combination = " + combination.ToString(), DBNull.Value, dv[4]["JH_Profit"]);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 6. Combination = " + combination.ToString(), DBNull.Value, dv[5]["JH_Profit"]);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 7. Combination = " + combination.ToString(), DBNull.Value, dv[6]["JH_Profit"]);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 8. Combination = " + combination.ToString(), DBNull.Value, dv[7]["JH_Profit"]);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 9. Combination = " + combination.ToString(), DBNull.Value, dv[8]["JH_Profit"]);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 10. Combination = " + combination.ToString(), 69M, dv[9]["JH_Profit"]);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 11. Combination = " + combination.ToString(), DBNull.Value, dv[10]["JH_Profit"]);
			AssertEquals("Profit sum should be shown on last line of charge code group. Checking line 12. Combination = " + combination.ToString(), -10M, dv[11]["JH_Profit"]);
		}

		public void TestLineBranchFilterProfitCalculation()
		{
			var excludedBranchPK = Guid.NewGuid();
			RunSQL(new object[] { excludedBranchPK, "EXC", CompanyPK }, "INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES({0}, {1}, {2})");
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 117, 100, true);
			CreateJobChargeAndLines(commonChargeCode, excludedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 85, 89, true);

			var dv = RunReportStoredProcedure(new[] { includedBranchPK });
			dv.Sort = "JH_Profit DESC";
			AssertEquals("Number of rows returned", 2, dv.Count);
			var profit = dv[0]["JH_Profit"];
			AssertEquals("Profit is correct", 17M, profit);
		}

		public void TestLineDepartmentFilterProfitCalculation()
		{
			var excludedDeptartmentPK = Guid.NewGuid();
			RunSQL(new object[] { excludedDeptartmentPK, "EXC" }, "INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code) VALUES({0}, {1})");
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 117, 100, true);
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, excludedDeptartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 85, 89, true);

			var dv = RunReportStoredProcedure(null, new[] { includedDepartmentPK });
			dv.Sort = "JH_Profit DESC";
			AssertEquals("Number of rows returned", 2, dv.Count);
			var profit = dv[0]["JH_Profit"];
			AssertEquals("Profit is correct", 17M, profit);
		}

		public void TestLineDebtorFilterProfitCalculation()
		{
			var excludedDebtorOrgPK = Guid.NewGuid();
			RunSQL(new object[] { excludedDebtorOrgPK, "ExcDebtor", "DEBT" }, "INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES ({0}, {1}, {2})");
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 117, 100, true);
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, excludedDebtorOrgPK, includedCreditorOrgPK, 85, 89, true);

			var dv = RunReportStoredProcedure(null, null, new[] { includedDebtorOrgPK });
			dv.Sort = "JH_Profit DESC";
			AssertEquals("Number of rows returned", 1, dv.Count);
			var profit = dv[0]["JH_Profit"];
			AssertEquals("Profit is correct", 117M, profit);
		}

		public void TestLineCreditorFilterProfitCalculation()
		{
			var excludedCreditorOrgPK = Guid.NewGuid();
			RunSQL(new object[] { excludedCreditorOrgPK, "ExcCreditor", "CRED" }, "INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES ({0}, {1}, {2})");
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 117, 100, true);
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, excludedCreditorOrgPK, 85, 89, true);

			var dv = RunReportStoredProcedure(null, null, null, new[] { includedCreditorOrgPK });
			dv.Sort = "JH_Profit DESC";
			AssertEquals("Number of rows returned", 1, dv.Count);
			var profit = dv[0]["JH_Profit"];
			AssertEquals("Profit is correct", -100M, profit);
		}

		public void TestProfitSummarisedByJobAndFilterByConsolAndGroupByCharge()
		{
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 117, 100, true);
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 115, 113, true, "2012-07-13", null);
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 30, 20, true, "2012-06-13", "2012-07-13");
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 55, 5, false, "2012-07-13", "2012-07-13");
			CreateJobChargeAndLines(commonChargeCode2, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedDebtorOrgPK, 21, 31, true, "2012-07-13", null);

			// Case when filter by consol ETD.
			// GroupByCharge property is always 'Y' when run report 'Job Profit - Forwarding.xls'.
			var dataView = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", false, false, false, "2017-04-30 00:00:00", null, true, true);
			AssertEquals(2, dataView.Count);
			foreach (DataRowView row in dataView)
			{
				AssertFilterByConsolColumns(row);
			}
			AssertContainsInAnyOrdarAndUnique(dataView, "AC_Code", new List<object>() { "CC1", "CC2" });

			dataView = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", false, false, false, "2017-08-01 00:00:00", null, true, true);
			AssertEquals(0, dataView.Count);

			// Case when NOT filter by consol ETD.
			dataView = RunReportStoredProcedure(null, null, null, null, "2012-07-01 00:00:00", "2012-07-31 00:00:00", false, false, false, null, null, true, true);
			AssertEquals(2, dataView.Count);
			foreach (DataRowView row in dataView)
			{
				AssertFilterByConsolColumns(row);
			}
			AssertContainsInAnyOrdarAndUnique(dataView, "AC_Code", new List<object>() { "CC1", "CC2" });
		}

		void AssertFilterByConsolColumns(DataRowView dataRow)
		{
			AssertEquals(1, dataRow["FW_FCLContainerCount"]);
			AssertEquals(3m, dataRow["FW_FCLContainerTEU"]);
			AssertEquals(STD("2017-07-02 00:00:00"), dataRow["FW_ConsolATA"]);
			AssertEquals(STD("2017-05-02 00:00:00"), dataRow["FW_ConsolATD"]);
			AssertEquals(STD("2017-07-01 00:00:00"), dataRow["FW_ConsolETA"]);
			AssertEquals(STD("2017-05-01 00:00:00"), dataRow["FW_ConsolETD"]);
			AssertEquals("TPL", dataRow["FW_ConsolFirstLoad"]);
			AssertEquals("TPD", dataRow["FW_ConsolLastDischarge"]);
		}
		void AssertContainsInAnyOrdarAndUnique(DataView dataView, string columnName, IEnumerable<object> expected)
		{
			var expectedCopy = expected.ToList();
			foreach (DataRowView row in dataView)
			{
				var actual = row[columnName];
				Assert(expectedCopy.Contains(actual));
				expectedCopy.Remove(actual);
			}
		}

		#region ShowSQL Parameter

		public void TestShowSQLParameter_WhenUnspecifiedAndSuccessfulQuery()
		{
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 30, 20, false, "2012-07-13", "2012-07-13");

			var command = CreateReportCommand();
			var results = RunReportStoredProcedureWithMultipleResultSets(command).ToArray();

			AssertEquals("One result set expected", 1, results.Length);

			var firstResult = results[0];
			AssertEquals("Two report rows expected", 2, firstResult.Count);
			AssertGreaterThan("Many columns expected in report rows", firstResult.Count, 1);
		}

		public void TestShowSQLParameter_WhenNAndSuccessfulQuery()
		{
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 30, 20, false, "2012-07-13", "2012-07-13");

			var command = CreateReportCommand();
			command.AddParameter("@ShowSQL", SqlDbType.Char, 1, 'N');
			var results = RunReportStoredProcedureWithMultipleResultSets(command).ToArray();

			AssertEquals("One result set expected", 1, results.Length);

			var firstResult = results[0];
			AssertEquals("Two report rows expected", 2, firstResult.Count);
			AssertGreaterThan("Many columns expected in report rows", firstResult.Count, 1);
		}

		public void TestShowSQLParameter_WhenYAndSuccessfulQuery()
		{
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 30, 20, false, "2012-07-13", "2012-07-13");

			var command = CreateReportCommand();
			command.AddParameter("@ShowSQL", SqlDbType.Char, 1, 'Y');
			var results = RunReportStoredProcedureWithMultipleResultSets(command).ToArray();

			AssertEquals("Three result sets expected", 3, results.Length);

			var firstResult = results[0];
			AssertEquals("One show SQL row expected before report result with 'Y' parameter", 1, firstResult.Count);
			AssertEquals("One show SQL column expected before report result with 'Y' parameter", 1, firstResult.Table.Columns.Count);
			AssertStartsWith("Generated SQL should start with SELECT", "SELECT", Convert.ToString(firstResult[0][0]).Trim());

			var secondResult = results[1];
			AssertEquals("Two report rows expected", 2, secondResult.Count);
			AssertGreaterThan("Many columns expected in report rows", secondResult.Table.Columns.Count, 1);

			var thirdResult = results[2];
			AssertEquals("One show SQL row expected after report result with 'Y' parameter", 1, thirdResult.Count);
			AssertEquals("One show SQL column expected after report result with 'Y' parameter", 1, thirdResult.Table.Columns.Count);
			AssertStartsWith("Generated SQL should start with SELECT", "SELECT", Convert.ToString(thirdResult[0][0]).Trim());
		}

		public void TestShowSQLParameter_WhenBAndSuccessfulQuery()
		{
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 30, 20, false, "2012-07-13", "2012-07-13");

			var command = CreateReportCommand();
			command.AddParameter("@ShowSQL", SqlDbType.Char, 1, 'B');
			var results = RunReportStoredProcedureWithMultipleResultSets(command).ToArray();

			AssertEquals("Two result sets expected", 2, results.Length);

			var firstResult = results[0];
			AssertEquals("One show SQL row expected before report result with 'B' parameter", 1, firstResult.Count);
			AssertEquals("One show SQL column expected before report result with 'B' parameter", 1, firstResult.Table.Columns.Count);
			AssertStartsWith("Generated SQL should start with SELECT", "SELECT", Convert.ToString(firstResult[0][0]).Trim());

			var secondResult = results[1];
			AssertEquals("Two report rows expected", 2, secondResult.Count);
			AssertGreaterThan("Many columns expected in report rows", secondResult.Table.Columns.Count, 1);
		}

		public void TestShowSQLParameter_WhenAAndSuccessfulQuery()
		{
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 30, 20, false, "2012-07-13", "2012-07-13");

			var command = CreateReportCommand();
			command.AddParameter("@ShowSQL", SqlDbType.Char, 1, 'A');
			var results = RunReportStoredProcedureWithMultipleResultSets(command).ToArray();

			AssertEquals("Two result sets expected", 2, results.Length);

			var firstResult = results[0];
			AssertEquals("Two report rows expected", 2, firstResult.Count);
			AssertGreaterThan("Many columns expected in report rows", firstResult.Table.Columns.Count, 1);

			var secondResult = results[1];
			AssertEquals("One show SQL row expected after report result with 'A' parameter", 1, secondResult.Count);
			AssertEquals("One show SQL column expected after report result with 'A' parameter", 1, secondResult.Table.Columns.Count);
			AssertStartsWith("Generated SQL should start with SELECT", "SELECT", Convert.ToString(secondResult[0][0]).Trim());
		}

		public void TestShowSQLParameter_WhenYAndNullQuery()
		{
			CreateJobChargeAndLines(commonChargeCode, includedBranchPK, includedDepartmentPK, includedDebtorOrgPK, includedCreditorOrgPK, 30, 20, false, "2012-07-13", "2012-07-13");

			var sql = $@"
EXEC Report_JobProfit 
@IncludeJobDetails = 'Y', 
@GROUPBYJOB = 'Y', 
@JH_GC = '{CompanyPK}',
@CurrentCountry = 'AU',
@AL_FromDate = '1900-01-01 00:00:00',
@AL_ToDate = '2079-06-06 23:59:29',
@Gateway = 'N',
@ShowSQL = 'Y'";
			var command = TestConnection.Command(sql);
			var results = RunReportStoredProcedureWithMultipleResultSets(command).ToArray();

			AssertEquals("Two result sets expected", 2, results.Length);

			var firstResult = results[0];
			AssertEquals("One show SQL row expected before report result with 'Y' parameter", 1, firstResult.Count);
			AssertEquals("One show SQL column expected before report result with 'Y' parameter", 1, firstResult.Table.Columns.Count);
			AssertEquals("Generated SQL should be NULL / empty", string.Empty, Convert.ToString(firstResult[0][0]).Trim());

			// If you mess the dynamic SQL up and try EXEC(NULL), you get no result set.

			var secondResult = results[1];
			AssertEquals("One show SQL row expected after report result with 'Y' parameter", 1, secondResult.Count);
			AssertEquals("One show SQL column expected after report result with 'Y' parameter", 1, secondResult.Table.Columns.Count);
			AssertEquals("Generated SQL should be NULL / empty", string.Empty, Convert.ToString(secondResult[0][0]).Trim());
		}

		#endregion

		#region Helper Methods

		DbCommand CreateReportCommand(
			IEnumerable<Guid> branchPKs = null,
			IEnumerable<Guid> departmentPKs = null,
			IEnumerable<Guid> debtorPKs = null,
			IEnumerable<Guid> creditorPKs = null,
			string alFromDate = "1900-01-01 00:00:00",
			string alToDate = "2079-06-06 23:59:29",
			bool outstandingWIPOnly = false,
			bool outstandingACROnly = false,
			bool includeConsolDetails = false,
			string consolFromETD = null,
			string jobType = null,
			bool isSummaryByJob = false,
			bool isGroupByCharge = false,
			string isActive = "All"
			)
		{
			var sqlCommand = TestConnection.Command("Report_JobProfit");
			sqlCommand.CommandType = CommandType.StoredProcedure;
			sqlCommand.AddParameter("@JH_GC", SqlDbType.UniqueIdentifier, CompanyPK);
			sqlCommand.AddParameter("@IncludeJobDetails", SqlDbType.VarChar, "Y");
			sqlCommand.AddParameter("@JobType", SqlDbType.VarChar, jobType ?? "");
			sqlCommand.AddParameter("@JH_BranchPKList", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@JH_DepartmentPKList", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@JH_FromCreatedDate", SqlDbType.DateTime, STD("1900-01-01 00:00:00"));
			sqlCommand.AddParameter("@JH_ToCreatedDate", SqlDbType.DateTime, STD("2079-06-06 23:59:29"));
			sqlCommand.AddParameter("@JH_FromClosedDate", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@JH_ToClosedDate", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@JH_FromRevenueRecognizedDate", SqlDbType.DateTime, STD("1900-01-01 00:00:00"));
			sqlCommand.AddParameter("@JH_ToRevenueRecognizedDate", SqlDbType.DateTime, STD("2079-06-06 23:59:29"));
			sqlCommand.AddParameter("@JH_LocalClientPKList", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@JH_OverseasAgentPKList", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@AL_OutstandingWIPOnly", SqlDbType.VarChar, outstandingWIPOnly ? "Y" : "");
			sqlCommand.AddParameter("@AL_OutstandingACROnly", SqlDbType.VarChar, outstandingACROnly ? "Y" : "");
			sqlCommand.AddParameter("@AL_FromDate", SqlDbType.DateTime, STD(alFromDate));
			sqlCommand.AddParameter("@AL_ToDate", SqlDbType.DateTime, STD(alToDate));
			sqlCommand.AddParameter("@AL_ExcludeReversedWIPACR", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@FW_FromETD", SqlDbType.DateTime, STD("1900-01-01 00:00:00"));
			sqlCommand.AddParameter("@FW_ToETD", SqlDbType.DateTime, STD("2079-06-06 23:59:29"));
			sqlCommand.AddParameter("@FW_FromETA", SqlDbType.DateTime, STD("1900-01-01 00:00:00"));
			sqlCommand.AddParameter("@FW_ToETA", SqlDbType.DateTime, STD("2079-06-06 23:59:29"));
			sqlCommand.AddParameter("@FW_ToRegistration", SqlDbType.DateTime, STD("2079-06-06 23:59:29"));
			sqlCommand.AddParameter("@FW_FromRegistration", SqlDbType.DateTime, STD("1900-01-01 00:00:00"));
			sqlCommand.AddParameter("@JK_ContainerMode", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@JK_TransportMode", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@JK_AgentType", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@JK_JX_FromETD", SqlDbType.DateTime, STD(consolFromETD ?? "1900-01-01 00:00:00"));
			sqlCommand.AddParameter("@JK_JX_ToETD", SqlDbType.DateTime, STD("2079-06-06 23:59:29"));
			sqlCommand.AddParameter("@JK_JX_FromETA", SqlDbType.DateTime, STD("1900-01-01 00:00:00"));
			sqlCommand.AddParameter("@JK_JX_ToETA", SqlDbType.DateTime, STD("2079-06-06 23:59:29"));
			sqlCommand.AddParameter("@CurrentCountry", SqlDbType.VarChar, "AU");
			sqlCommand.AddParameter("@AL_ChargeCodePKList", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@AL_ExcludedChargeCodePKList", SqlDbType.VarChar, "");
			sqlCommand.AddParameter("@IsSummaryByJob", SqlDbType.VarChar, isSummaryByJob ? 'Y' : 'N');
			sqlCommand.AddParameter("@GroupByCharge", SqlDbType.VarChar, isGroupByCharge ? 'Y' : 'N');
			sqlCommand.AddParameter("@Gateway", SqlDbType.VarChar, "ALL");
			sqlCommand.AddParameter("@JH_IsActive", SqlDbType.VarChar, isActive);

			if (includeConsolDetails)
			{
				sqlCommand.AddParameter("@IncludeConsolDetails", SqlDbType.VarChar, "Y");
			}

			if (branchPKs != null) { sqlCommand.AddParameter("@AL_BranchPKList", SqlDbType.VarChar, FormatIdList(branchPKs)); }
			if (departmentPKs != null) { sqlCommand.AddParameter("@AL_DepartmentPKList", SqlDbType.VarChar, FormatIdList(departmentPKs)); }
			if (creditorPKs != null) { sqlCommand.AddParameter("@AL_CreditorPKList", SqlDbType.VarChar, FormatIdList(creditorPKs)); }
			if (debtorPKs != null) { sqlCommand.AddParameter("@AL_DebtorPKList", SqlDbType.VarChar, FormatIdList(debtorPKs)); }

			return sqlCommand;
		}

		DataView RunReportStoredProcedure(
			IEnumerable<Guid> branchPKs = null,
			IEnumerable<Guid> departmentPKs = null,
			IEnumerable<Guid> debtorPKs = null,
			IEnumerable<Guid> creditorPKs = null,
			string alFromDate = "1900-01-01 00:00:00",
			string alToDate = "2079-06-06 23:59:29",
			bool outstandingWIPOnly = false,
			bool outstandingACROnly = false,
			bool includeConsolDetails = false,
			string consolFromETD = null,
			string jobType = null,
			bool isSummaryByJob = false,
			bool isGroupByCharge = false,
			string isActive = "All"
			)
		{
			var sqlCommand = CreateReportCommand(
				branchPKs: branchPKs,
				departmentPKs: departmentPKs,
				debtorPKs: debtorPKs,
				creditorPKs: creditorPKs,
				alFromDate: alFromDate,
				alToDate: alToDate,
				outstandingWIPOnly: outstandingWIPOnly,
				outstandingACROnly: outstandingACROnly,
				includeConsolDetails: includeConsolDetails,
				consolFromETD: consolFromETD,
				jobType: jobType,
				isSummaryByJob: isSummaryByJob,
				isGroupByCharge: isGroupByCharge,
				isActive: isActive
				);

			return RunReportStoredProcedureWithMultipleResultSets(sqlCommand).First();
		}

		IEnumerable<DataView> RunReportStoredProcedureWithMultipleResultSets(DbCommand command)
		{
			using (var reader = command.ExecuteReader())
			{
				do
				{
					var dt = new DataTable();
					dt.Load(reader);
					yield return new DataView(dt);
				} while (!reader.IsClosed);
			}
		}

		protected DateTime STD(string dateString)
		{
			return DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", null);
		}

		protected string FormatIdList(IEnumerable<Guid> guids)
		{
			return string.Join(",", (from Guid guid in guids select string.Format("'{0}'", guid.ToString("D"))).ToArray());
		}

		protected void RunSQL(object[] values, string sql)
		{
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i] == null)
				{
					values[i] = "NULL";
				}
				else if (values[i] is Guid)
				{
					values[i] = string.Format("'{0:d}'", values[i]);
				}
				else if (values[i] is string)
				{
					values[i] = string.Format("'{0}'", values[i]);
				}
			}

			TestConnection.ExecuteNonQuery(string.Format(sql, values));
		}

		#endregion

		#region Implementation
		protected readonly Guid CompanyPK = new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");

		Guid includedCreditorOrgPK;
		Guid includedDebtorOrgPK;
		Guid includedBranchPK;
		Guid includedDepartmentPK;
		Guid jobShipmentPK;
		Guid commonChargeCode;
		Guid commonChargeCode2;
		int nextInvNumber = 1;
		Guid jobPK;

		protected override void SetUp()
		{
			base.SetUp();

			includedCreditorOrgPK = Guid.NewGuid();
			includedDebtorOrgPK = Guid.NewGuid();
			includedBranchPK = Guid.NewGuid();
			includedDepartmentPK = Guid.NewGuid();
			jobShipmentPK = Guid.NewGuid();
			commonChargeCode = Guid.NewGuid();
			commonChargeCode2 = Guid.NewGuid();
			jobPK = Guid.NewGuid();
			var consolPK = Guid.NewGuid();

			var jobConsolPK = Guid.NewGuid();
			var jobConsolTransportPK = Guid.NewGuid();

			var jobConShipLinkPK = Guid.NewGuid();
			var jobPackLinesPK = Guid.NewGuid();
			var refContainerPK = Guid.NewGuid();
			var jobContainerPK = Guid.NewGuid();
			var jobContainerPackPivotPK = Guid.NewGuid();

			RunSQL(new object[] { commonChargeCode, "CC1", CompanyPK }, "Insert INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeType, AC_MarginPercentage, AC_ChargeGroup, AC_GC, AC_SystemCreateTimeUtc, AC_SystemCreateUser, AC_SystemLastEditTimeUtc, AC_SystemLastEditUser) VALUES ({0},{1},'MRG',100,'ORG',{2}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			RunSQL(new object[] { commonChargeCode2 = Guid.NewGuid(), "CC2", CompanyPK }, "Insert INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeType, AC_MarginPercentage, AC_ChargeGroup, AC_GC, AC_SystemCreateTimeUtc, AC_SystemCreateUser, AC_SystemLastEditTimeUtc, AC_SystemLastEditUser) VALUES ({0},{1},'MRG',100,'ORG',{2}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			RunSQL(new object[] { includedCreditorOrgPK, "Creditor", "CRED" }, "INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) VALUES ({0}, {1}, {2}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			RunSQL(new object[] { includedDebtorOrgPK, "Debtor", "DEBT" }, "INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) VALUES ({0}, {1}, {2}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			RunSQL(new object[] { includedBranchPK, "INC", CompanyPK }, "INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES({0}, {1}, {2})");
			RunSQL(new object[] { includedDepartmentPK, "INC" }, "INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code) VALUES({0}, {1})");
			RunSQL(new object[] { jobShipmentPK, "S00001234", "2012-07-13", "FCL", "BCN" }, "INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsForwardRegistered, JS_SystemCreateTimeUtc, JS_PackingMode, JS_ShipmentType, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser) VALUES ({0}, {1}, 1, {2}, {3}, {4}, '~BP', GetUtcDate(), '~BP')");
			RunSQL(new object[] { jobPK, "J1", includedBranchPK, CompanyPK, includedDepartmentPK, jobShipmentPK, "WRK", "JS", "2012-07-13" }, "INSERT INTO dbo.JobHeader (JH_PK, JH_JobNum, JH_GB, JH_GC, JH_GE, JH_ParentID, JH_Status, JH_ParentTableCode, JH_SystemCreateTimeUtc, JH_SystemCreateUser, JH_SystemLastEditTimeUtc, JH_SystemLastEditUser) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, '~BP', GetUtcDate(), '~BP')");

			RunSQL(new object[] { jobConsolPK, "XXX" }, "INSERT INTO dbo.JobConsol (JK_PK, JK_AgentType, JK_SystemCreateTimeUtc, JK_SystemCreateUser, JK_SystemLastEditTimeUtc, JK_SystemLastEditUser) VALUES ({0}, {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			RunSQL(new object[] { jobConShipLinkPK, jobConsolPK, jobShipmentPK }, @"INSERT INTO dbo.JobConShipLink (JN_PK, JN_JK, JN_JS, JN_SystemCreateTimeUtc, JN_SystemCreateUser, JN_SystemLastEditTimeUtc, JN_SystemLastEditUser) VALUES ({0}, {1}, {2}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			RunSQL(new object[] { jobConsolTransportPK, jobConsolPK },
			@"INSERT INTO dbo.JobConsolTransport
			(JW_PK, JW_IsValid, JW_TransportMode, JW_LegOrder, JW_TransportType, 
			 JW_Status, JW_Vessel, JW_VoyageFlight,
			 JW_RL_NKLoadPort, JW_RL_NKDiscPort,
			 JW_ETD, JW_ATD, JW_ETA, JW_ATA, 
			 JW_ParentType, JW_ParentGUID, JW_SystemCreateTimeUtc, JW_SystemCreateUser, JW_SystemLastEditTimeUtc, JW_SystemLastEditUser)
			VALUES
			({0}, 1, 'SEA', 1, 'MAI',
			 'CNF', 'TSV', '',
			 'TPL', 'TPD',
			 '2017-05-01', '2017-05-02', '2017-07-01', '2017-07-02',
			 'CON', {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP'
			)");

			RunSQL(new object[] { jobPackLinesPK, jobShipmentPK }, "insert dbo.JobPackLines(JL_PK, JL_JS, JL_SystemCreateTimeUtc, JL_SystemCreateUser, JL_SystemLastEditTimeUtc, JL_SystemLastEditUser) VALUES({0}, {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			RunSQL(new object[] { refContainerPK, "TSC", 3 }, "insert dbo.RefContainer(RC_PK, RC_Code, RC_TEU) VALUES({0}, {1}, {2})");
			RunSQL(new object[] { jobContainerPK, refContainerPK }, "insert dbo.JobContainer(JC_PK, JC_RC, JC_SystemCreateTimeUtc, JC_SystemCreateUser, JC_SystemLastEditTimeUtc, JC_SystemLastEditUser) VALUES({0}, {1}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			RunSQL(new object[] { jobContainerPackPivotPK, jobPackLinesPK, jobContainerPK }, "insert dbo.JobContainerPackPivot(J6_PK, J6_JL, J6_JC, J6_SystemCreateTimeUtc, J6_SystemCreateUser, J6_SystemLastEditTimeUtc, J6_SystemLastEditUser) VALUES({0}, {1}, {2}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
		}

		protected void CreateJobChargeAndLines(Guid chargeCodePK, Guid branchPK, Guid departmentPK, Guid debtorOrgPK, Guid creditorOrgPK, decimal revenueOrWipAmount, decimal costOrAccrualAmount, bool isWipAccrual, string postDate = "2012-07-13", string reverseDate = null)
		{
			Guid transactionLinePKRev;
			Guid transactionLinePKCst;
			var transactionHeaderPK = Guid.NewGuid();
			RunSQL(new object[] { transactionHeaderPK, nextInvNumber++.ToString(), branchPK, departmentPK }, "INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_TransactionReference,AH_Desc,AH_InvoiceDate,AH_TransactionCategory,AH_DueDate,AH_InvoiceAmount,AH_GSTAmount,AH_WithholdingTax,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_AgePeriod,AH_PostPeriod,AH_PostDate,AH_ChequeOrReference,AH_ReceiptType,AH_CashBasisGSTIndicator,AH_CashBasisGSTRealisedToGL,AH_ChequeDrawer,AH_DrawerBank,AH_DrawerBranch,AH_InvoiceApproved,AH_ConsolidatedInvoiceRef,AH_FullyPaidDate,AH_InvoicePrinted,AH_IsCancelled,AH_DateClearedInCashbook,AH_NotAllocated,AH_OutstandingAmount,AH_PostedToEFT,AH_PostToGL,AH_ReceiptBatchNo,AH_TransactionCreatedByMatching,AH_InvoiceTerm,AH_InvoiceTermDays,AH_POST1,AH_POST2,AH_POST3,AH_POST4,AH_AB,AH_OH,AH_JH,AH_GB,AH_GE,AH_AG,AH_TransactionBelongsToGroup,AH_AH_InvoiceStatement,AH_PostedInternal,AH_GC)VALUES({0},'AP','DSC',{1},1,'','DIRECT PAYMENT','May 25 2005  3:44:00:000PM','','May 25 2005  3:44:00:000PM',-30.0000,-1.0000,0.0000,-31.0000,'AUD',1.000000000,0,0,'May 25 2005  3:44:00:000PM','CASH','CSH',0,0,'CASH','','',0,'',NULL,0,0,NULL,0,0.0000,0,'Y','',0,'',0,0,0,0,0,NULL,NULL,NULL,{2},{3},NULL,NULL,NULL,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			RunSQL(new object[] { transactionLinePKRev = Guid.NewGuid(), transactionHeaderPK, jobPK, branchPK, departmentPK, chargeCodePK, (isWipAccrual ? -1 : 1) * revenueOrWipAmount, isWipAccrual ? "WIP" : "REV", debtorOrgPK, postDate, reverseDate, CompanyPK, DbHelper.GLAccountPK1 }, "insert into dbo.Acctransactionlines (AL_PK, AL_AH, AL_JH, AL_GB, AL_GE, AL_AC, AL_LineAmount, AL_LineType, AL_OH, AL_PostDate, AL_ReverseDate, AL_GC, AL_AG) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})");
			RunSQL(new object[] { transactionLinePKCst = Guid.NewGuid(), transactionHeaderPK, jobPK, branchPK, departmentPK, chargeCodePK, (isWipAccrual ? 1 : -1) * costOrAccrualAmount, isWipAccrual ? "ACR" : "CST", creditorOrgPK, postDate, reverseDate, CompanyPK, DbHelper.GLAccountPK1 }, "insert into dbo.Acctransactionlines (AL_PK, AL_AH, AL_JH, AL_GB, AL_GE, AL_AC, AL_LineAmount, AL_LineType, AL_OH, AL_PostDate, AL_ReverseDate, AL_GC, AL_AG) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})");

			if (isWipAccrual && reverseDate != null)
			{
				// Create the revenue and cost lines that caused this reversal of WIP and ACR
				RunSQL(new object[] { transactionLinePKRev = Guid.NewGuid(), transactionHeaderPK, jobPK, branchPK, departmentPK, chargeCodePK, revenueOrWipAmount, "REV", debtorOrgPK, reverseDate, reverseDate, CompanyPK, DbHelper.GLAccountPK1 }, "insert into dbo.Acctransactionlines (AL_PK, AL_AH, AL_JH, AL_GB, AL_GE, AL_AC, AL_LineAmount, AL_LineType, AL_OH, AL_PostDate, AL_ReverseDate, AL_GC, AL_AG) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})");
				RunSQL(new object[] { transactionLinePKCst = Guid.NewGuid(), transactionHeaderPK, jobPK, branchPK, departmentPK, chargeCodePK, -costOrAccrualAmount, "CST", creditorOrgPK, reverseDate, reverseDate, CompanyPK, DbHelper.GLAccountPK1 }, "insert into dbo.Acctransactionlines (AL_PK, AL_AH, AL_JH, AL_GB, AL_GE, AL_AC, AL_LineAmount, AL_LineType, AL_OH, AL_PostDate, AL_ReverseDate, AL_GC, AL_AG) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})");
			}

			RunSQL(new object[] { Guid.NewGuid(), jobPK, branchPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst, CompanyPK }, "Insert INTO dbo.JobCharge (JR_PK, JR_JH, JR_GB, JR_GE, JR_AC, JR_AL_ARLine, JR_AL_APLine, JR_GC, JR_SystemLastEditTimeUtc, JR_SystemLastEditUser) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, GETUTCDATE(), '~BP')");
		}
		#endregion
	}
}

