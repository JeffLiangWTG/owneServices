
IF OBJECT_ID('tempdb..#CognosAccounts') IS NOT NULL
    DROP TABLE #CognosAccounts

CREATE TABLE #CognosAccounts
(
    T1_AG           uniqueidentifier,
    T1_Code         nvarchar(40),
    T1_Name         nvarchar(160)
)

INSERT INTO #CognosAccounts
SELECT  AJ_AG AS T1_GLAccount, 
        LEFT(AJ_LocalAccountNumber, CHARINDEX('.', AJ_LocalAccountNumber) - 1) AS T1_Code, 
        (
            SELECT  TOP 1 AJ_AccountDescription 
            FROM    dbo.AccGLAccountDescriptor B
            WHERE   LEFT(B.AJ_LocalAccountNumber, CHARINDEX('.', B.AJ_LocalAccountNumber)) =
					LEFT(A.AJ_LocalAccountNumber, CHARINDEX('.', A.AJ_LocalAccountNumber))
            AND     B.AJ_Language = 'ZZZ' 
        ) AS T1_Name
FROM    dbo.AccGLAccountDescriptor A
WHERE   AJ_Language = 'ZZZ'


IF OBJECT_ID('tempdb..#CounterCompanies') IS NOT NULL
    DROP TABLE #CounterCompanies

CREATE TABLE #CounterCompanies
(
    T2_OH               uniqueidentifier,
    T2_CompanyCode      nvarchar(8),
    T2_BusinessType     nvarchar(3),
    T2_Geographical     nvarchar(3)
)


INSERT INTO #CounterCompanies
SELECT  OH_PK AS T2_OH,
        CASE
            WHEN OK_OH IS NULL THEN 'ICTOTA'
            ELSE OK_CustomsRegNo
        END AS T2_CompanyCode,
        '' AS T2_BusinessType,
        CASE
            WHEN RZ_Code IN ('SAM') THEN 'SAM'
            WHEN RZ_Code IN ('NAM', 'USA') THEN 'NAM'
            WHEN RZ_Code IN ('SEA') THEN 'SEA'
            WHEN RZ_Code IN ('AUS', 'NZD') THEN 'ANZ'
            WHEN RZ_Code IN ('MEA') THEN 'MEA'
            WHEN RZ_Code IN ('EUO', 'EUR') THEN 'EUR'
            WHEN RZ_Code IN ('AFO', 'AFR', 'SAF') THEN 'AFR'
            WHEN RZ_Code IN ('AME', 'ASI', 'CAM', 'IND', 'NAS', 'OCE', 'PAC') THEN 'OTH'           
            ELSE ''
        END AS T2_Geographical
FROM    dbo.OrgHeader
        LEFT OUTER JOIN dbo.OrgCusCode ON OK_CodeType = 'UNC' AND OK_OH = OH_PK
        LEFT OUTER JOIN dbo.RefUNLOCO ON OH_RL_NKClosestPort = RL_Code
        LEFT OUTER JOIN RefZone ON RL_RZ = RZ_PK
WHERE   OH_PK IN 
        (
            SELECT DISTINCT AH_OH 
            FROM dbo.AccTransactionHeader
        )


IF OBJECT_ID('tempdb..#CognosModes') IS NOT NULL
    DROP TABLE #CognosModes

CREATE TABLE #CognosModes
(
    T3_GE           uniqueidentifier,
    T3_Mode         nvarchar(4)    
)

exec sp_executesql N'INSERT INTO #CognosModes VALUES (@T3_GE_0_0, @T3_Mode_0_0)
INSERT INTO #CognosModes VALUES (@T3_GE_0_1, @T3_Mode_0_1)
INSERT INTO #CognosModes VALUES (@T3_GE_0_2, @T3_Mode_0_2)
INSERT INTO #CognosModes VALUES (@T3_GE_0_3, @T3_Mode_0_3)
INSERT INTO #CognosModes VALUES (@T3_GE_0_4, @T3_Mode_0_4)
INSERT INTO #CognosModes VALUES (@T3_GE_0_5, @T3_Mode_0_5)
INSERT INTO #CognosModes VALUES (@T3_GE_0_6, @T3_Mode_0_6)
INSERT INTO #CognosModes VALUES (@T3_GE_0_7, @T3_Mode_0_7)
INSERT INTO #CognosModes VALUES (@T3_GE_0_8, @T3_Mode_0_8)
INSERT INTO #CognosModes VALUES (@T3_GE_0_9, @T3_Mode_0_9)
INSERT INTO #CognosModes VALUES (@T3_GE_0_10, @T3_Mode_0_10)
INSERT INTO #CognosModes VALUES (@T3_GE_0_11, @T3_Mode_0_11)
INSERT INTO #CognosModes VALUES (@T3_GE_0_12, @T3_Mode_0_12)
INSERT INTO #CognosModes VALUES (@T3_GE_0_13, @T3_Mode_0_13)
INSERT INTO #CognosModes VALUES (@T3_GE_0_14, @T3_Mode_0_14)
INSERT INTO #CognosModes VALUES (@T3_GE_0_15, @T3_Mode_0_15)
INSERT INTO #CognosModes VALUES (@T3_GE_0_16, @T3_Mode_0_16)
INSERT INTO #CognosModes VALUES (@T3_GE_0_17, @T3_Mode_0_17)
INSERT INTO #CognosModes VALUES (@T3_GE_0_18, @T3_Mode_0_18)
INSERT INTO #CognosModes VALUES (@T3_GE_0_19, @T3_Mode_0_19)
INSERT INTO #CognosModes VALUES (@T3_GE_0_20, @T3_Mode_0_20)
INSERT INTO #CognosModes VALUES (@T3_GE_0_21, @T3_Mode_0_21)
INSERT INTO #CognosModes VALUES (@T3_GE_0_22, @T3_Mode_0_22)
INSERT INTO #CognosModes VALUES (@T3_GE_0_23, @T3_Mode_0_23)
INSERT INTO #CognosModes VALUES (@T3_GE_0_24, @T3_Mode_0_24)
INSERT INTO #CognosModes VALUES (@T3_GE_0_25, @T3_Mode_0_25)
INSERT INTO #CognosModes VALUES (@T3_GE_0_26, @T3_Mode_0_26)
INSERT INTO #CognosModes VALUES (@T3_GE_0_27, @T3_Mode_0_27)
INSERT INTO #CognosModes VALUES (@T3_GE_0_28, @T3_Mode_0_28)
INSERT INTO #CognosModes VALUES (@T3_GE_0_29, @T3_Mode_0_29)
INSERT INTO #CognosModes VALUES (@T3_GE_0_30, @T3_Mode_0_30)
INSERT INTO #CognosModes VALUES (@T3_GE_0_31, @T3_Mode_0_31)
INSERT INTO #CognosModes VALUES (@T3_GE_0_32, @T3_Mode_0_32)
INSERT INTO #CognosModes VALUES (@T3_GE_0_33, @T3_Mode_0_33)
INSERT INTO #CognosModes VALUES (@T3_GE_0_34, @T3_Mode_0_34)
INSERT INTO #CognosModes VALUES (@T3_GE_0_35, @T3_Mode_0_35)
INSERT INTO #CognosModes VALUES (@T3_GE_0_36, @T3_Mode_0_36)
INSERT INTO #CognosModes VALUES (@T3_GE_0_37, @T3_Mode_0_37)
INSERT INTO #CognosModes VALUES (@T3_GE_0_38, @T3_Mode_0_38)
INSERT INTO #CognosModes VALUES (@T3_GE_0_39, @T3_Mode_0_39)
INSERT INTO #CognosModes VALUES (@T3_GE_0_40, @T3_Mode_0_40)
INSERT INTO #CognosModes VALUES (@T3_GE_0_41, @T3_Mode_0_41)
INSERT INTO #CognosModes VALUES (@T3_GE_0_42, @T3_Mode_0_42)
INSERT INTO #CognosModes VALUES (@T3_GE_0_43, @T3_Mode_0_43)
INSERT INTO #CognosModes VALUES (@T3_GE_0_44, @T3_Mode_0_44)
INSERT INTO #CognosModes VALUES (@T3_GE_0_45, @T3_Mode_0_45)
INSERT INTO #CognosModes VALUES (@T3_GE_0_46, @T3_Mode_0_46)
INSERT INTO #CognosModes VALUES (@T3_GE_0_47, @T3_Mode_0_47)
INSERT INTO #CognosModes VALUES (@T3_GE_0_48, @T3_Mode_0_48)
INSERT INTO #CognosModes VALUES (@T3_GE_0_49, @T3_Mode_0_49)
',N'@T3_GE_0_0 uniqueidentifier,@T3_Mode_0_0 nvarchar(2),@T3_GE_0_1 uniqueidentifier,@T3_Mode_0_1 nvarchar(3),@T3_GE_0_2 uniqueidentifier,@T3_Mode_0_2 nvarchar(2),@T3_GE_0_3 
uniqueidentifier,@T3_Mode_0_3 nvarchar(4000),@T3_GE_0_4 uniqueidentifier,@T3_Mode_0_4 nvarchar(3),@T3_GE_0_5 uniqueidentifier,@T3_Mode_0_5 nvarchar(2),@T3_GE_0_6 
uniqueidentifier,@T3_Mode_0_6 nvarchar(3),@T3_GE_0_7 uniqueidentifier,@T3_Mode_0_7 nvarchar(4000),@T3_GE_0_8 uniqueidentifier,@T3_Mode_0_8 nvarchar(2),@T3_GE_0_9 
uniqueidentifier,@T3_Mode_0_9 nvarchar(3),@T3_GE_0_10 uniqueidentifier,@T3_Mode_0_10 nvarchar(2),@T3_GE_0_11 uniqueidentifier,@T3_Mode_0_11 nvarchar(2),@T3_GE_0_12 
uniqueidentifier,@T3_Mode_0_12 nvarchar(2),@T3_GE_0_13 uniqueidentifier,@T3_Mode_0_13 nvarchar(4000),@T3_GE_0_14 uniqueidentifier,@T3_Mode_0_14 nvarchar(3),@T3_GE_0_15 
uniqueidentifier,@T3_Mode_0_15 nvarchar(3),@T3_GE_0_16 uniqueidentifier,@T3_Mode_0_16 nvarchar(3),@T3_GE_0_17 uniqueidentifier,@T3_Mode_0_17 nvarchar(2),@T3_GE_0_18 
uniqueidentifier,@T3_Mode_0_18 nvarchar(4000),@T3_GE_0_19 uniqueidentifier,@T3_Mode_0_19 nvarchar(2),@T3_GE_0_20 uniqueidentifier,@T3_Mode_0_20 nvarchar(3),@T3_GE_0_21 
uniqueidentifier,@T3_Mode_0_21 nvarchar(2),@T3_GE_0_22 uniqueidentifier,@T3_Mode_0_22 nvarchar(3),@T3_GE_0_23 uniqueidentifier,@T3_Mode_0_23 nvarchar(4000),@T3_GE_0_24 
uniqueidentifier,@T3_Mode_0_24 nvarchar(3),@T3_GE_0_25 uniqueidentifier,@T3_Mode_0_25 nvarchar(3),@T3_GE_0_26 uniqueidentifier,@T3_Mode_0_26 nvarchar(2),@T3_GE_0_27 
uniqueidentifier,@T3_Mode_0_27 nvarchar(3),@T3_GE_0_28 uniqueidentifier,@T3_Mode_0_28 nvarchar(3),@T3_GE_0_29 uniqueidentifier,@T3_Mode_0_29 nvarchar(3),@T3_GE_0_30 
uniqueidentifier,@T3_Mode_0_30 nvarchar(2),@T3_GE_0_31 uniqueidentifier,@T3_Mode_0_31 nvarchar(2),@T3_GE_0_32 uniqueidentifier,@T3_Mode_0_32 nvarchar(2),@T3_GE_0_33 
uniqueidentifier,@T3_Mode_0_33 nvarchar(3),@T3_GE_0_34 uniqueidentifier,@T3_Mode_0_34 nvarchar(3),@T3_GE_0_35 uniqueidentifier,@T3_Mode_0_35 nvarchar(3),@T3_GE_0_36 
uniqueidentifier,@T3_Mode_0_36 nvarchar(3),@T3_GE_0_37 uniqueidentifier,@T3_Mode_0_37 nvarchar(3),@T3_GE_0_38 uniqueidentifier,@T3_Mode_0_38 nvarchar(3),@T3_GE_0_39 
uniqueidentifier,@T3_Mode_0_39 nvarchar(3),@T3_GE_0_40 uniqueidentifier,@T3_Mode_0_40 nvarchar(4000),@T3_GE_0_41 uniqueidentifier,@T3_Mode_0_41 nvarchar(3),@T3_GE_0_42 
uniqueidentifier,@T3_Mode_0_42 nvarchar(2),@T3_GE_0_43 uniqueidentifier,@T3_Mode_0_43 nvarchar(4000),@T3_GE_0_44 uniqueidentifier,@T3_Mode_0_44 nvarchar(3),@T3_GE_0_45 
uniqueidentifier,@T3_Mode_0_45 nvarchar(4000),@T3_GE_0_46 uniqueidentifier,@T3_Mode_0_46 nvarchar(4000),@T3_GE_0_47 uniqueidentifier,@T3_Mode_0_47 nvarchar(3),@T3_GE_0_48 
uniqueidentifier,@T3_Mode_0_48 nvarchar(3),@T3_GE_0_49 uniqueidentifier,@T3_Mode_0_49 
nvarchar(2)',@T3_GE_0_0='1E2C1BC2-1E57-4312-8A70-05B7B840B053',@T3_Mode_0_0=N'MI',@T3_GE_0_1='6B0DE052-1C4C-4B1A-8FE8-072E928D492B',@T3_Mode_0_1=N'CHB',@T3_GE_0_2='DB20A0C6-1FE5-42C6-8226-0982043F9E06',@T3_Mode_0_2=N'MI',@T3_GE_0_3='2B67864D-42E9-4A43-A9C8-09D2083C4227',@T3_Mode_0_3=N'',@T3_GE_0_4='237D028B-1864-4467-B34B-0AAC3F990C03',@T3_Mode_0_4=N'CHB',@T3_GE_0_5='7210286B-37BD-4054-A27E-0FA922D6EE0A',@T3_Mode_0_5=N'MI',@T3_GE_0_6='583890C5-CC83-4A43-8F5A-16D3FD6295CD',@T3_Mode_0_6=N'CHB',@T3_GE_0_7='66FB3C22-9E61-43AE-A9CA-17A2DF581049',@T3_Mode_0_7=N'',@T3_GE_0_8='570C4273-554E-4183-87C4-1AEEFEC801B7',@T3_Mode_0_8=N'ME',@T3_GE_0_9='B824E3DC-EACE-4A17-B868-1AF5724E3C56',@T3_Mode_0_9=N'CHB',@T3_GE_0_10='D1BEEFC5-111C-4D53-AAF7-1B877F64D7F4',@T3_Mode_0_10=N'AI',@T3_GE_0_11='AA8C4AFD-7413-4396-AFBF-1E7BF76CCB41',@T3_Mode_0_11=N'AI',@T3_GE_0_12='0D945500-257D-4AEA-956C-26D1B51FAF47',@T3_Mode_0_12=N'AI',@T3_GE_0_13='8EEB6773-D15C-47B2-AD51-2F499919A420',@T3_Mode_0_13=N'',@T3_GE_0_14='98241427-BEA2-4F31-B0C9-4566DEA3811E',@T3_Mode_0_14=N'CHB',@T3_GE_0_15='1C782850-1DD2-4C1B-9786-45755F897407',@T3_Mode_0_15=N'CHB',@T3_GE_0_16='6039945E-EDE5-40CC-9106-4F4A8E217759',@T3_Mode_0_16=N'CHB',@T3_GE_0_17='6ECA5FD0-DCCF-4136-A82E-57CF635D9BD1',@T3_Mode_0_17=N'AE',@T3_GE_0_18='4E5A97E8-85F4-41EC-95C6-5D14A676157C',@T3_Mode_0_18=N'',@T3_GE_0_19='22036533-0580-4D80-B90F-5F5FD5592B49',@T3_Mode_0_19=N'AE',@T3_GE_0_20='50D936A7-58DE-4D83-8284-614BCC17B118',@T3_Mode_0_20=N'CHB',@T3_GE_0_21='A46C47F6-45B5-4A95-95B5-615F28C04269',@T3_Mode_0_21=N'MI',@T3_GE_0_22='D29EAA21-0C14-4D60-9EDE-693FBAA84F25',@T3_Mode_0_22=N'CHB',@T3_GE_0_23='AB9930EB-0BF7-4C87-ADE1-6985B0226970',@T3_Mode_0_23=N'',@T3_GE_0_24='8C3B5BA4-4012-406A-9810-7BD6A746E6B6',@T3_Mode_0_24=N'CHB',@T3_GE_0_25='F5C72696-19AD-4759-879F-89C8532FF238',@T3_Mode_0_25=N'CHB',@T3_GE_0_26='D42DA9E0-DFE6-4405-A75C-8AC462333C14',@T3_Mode_0_26=N'AI',@T3_GE_0_27='5CD0B66A-A038-4507-935E-8CD62CCF0D9A',@T3_Mode_0_27=N'CHB',@T3_GE_0_28='01298D62-6BA9-49F2-985B-8F7279A34231',@T3_Mode_0_28=N'CHB',@T3_GE_0_29='0869AAB7-37BC-419C-A3D0-99A85511741C',@T3_Mode_0_29=N'CHB',@T3_GE_0_30='F5903A50-F7EE-4B18-914A-A13B5C4E91D6',@T3_Mode_0_30=N'MI',@T3_GE_0_31='432D350D-037E-4620-AEC8-A422DCC2FC7D',@T3_Mode_0_31=N'MI',@T3_GE_0_32='8C334817-BB6B-4EDB-B9B4-A57A17B23E13',@T3_Mode_0_32=N'ME',@T3_GE_0_33='57F778C1-DAF6-46E0-B7DC-AF01C161C936',@T3_Mode_0_33=N'CHB',@T3_GE_0_34='88DB4F58-FD9E-4FF7-9B7C-AFE614C72D06',@T3_Mode_0_34=N'WPT',@T3_GE_0_35='602FB270-515D-4D04-847C-B4CB7D1F18CE',@T3_Mode_0_35=N'CHB',@T3_GE_0_36='5A04047D-6027-4E48-BB41-B6640A863ABB',@T3_Mode_0_36=N'CHB',@T3_GE_0_37='66CA1C58-C63A-44A4-AB1E-B7067B435E79',@T3_Mode_0_37=N'CHB',@T3_GE_0_38='D1085AC9-4950-47CE-9211-BBC575A46C35',@T3_Mode_0_38=N'CHB',@T3_GE_0_39='A81A6208-8BBE-4669-B21E-BDF058677F00',@T3_Mode_0_39=N'CHB',@T3_GE_0_40='F32ED53D-2F04-4516-A14E-C0AE0A603F9D',@T3_Mode_0_40=N'',@T3_GE_0_41='3BC44454-A9C4-46A5-A5B1-C704C3605AE9',@T3_Mode_0_41=N'CHB',@T3_GE_0_42='86BB1C22-0865-4685-996E-D56CBD136491',@T3_Mode_0_42=N'AI',@T3_GE_0_43='06304D9A-309F-4A34-99E9-D63FFA135026',@T3_Mode_0_43=N'',@T3_GE_0_44='137CBBB6-2488-441A-A8BD-DD0B985891B7',@T3_Mode_0_44=N'CHB',@T3_GE_0_45='DD523EC8-0285-4726-AA86-DFC2C3AA2B1A',@T3_Mode_0_45=N'',@T3_GE_0_46='01DDFE6C-0DB6-4039-886D-E0128289869C',@T3_Mode_0_46=N'',@T3_GE_0_47='CE45A9D7-2064-42FE-8BA8-EAEBE1563B9D',@T3_Mode_0_47=N'CHB',@T3_GE_0_48='7A61D193-3B37-40DE-A915-F69C78464E24',@T3_Mode_0_48=N'CHB',@T3_GE_0_49='6C1CBB31-36C7-43C7-A29F-FD1B0F0050DC',@T3_Mode_0_49=N'AE'
exec sp_executesql N'INSERT INTO #CognosModes VALUES (@T3_GE_1_0, @T3_Mode_1_0)
',N'@T3_GE_1_0 uniqueidentifier,@T3_Mode_1_0 nvarchar(4000)',@T3_GE_1_0='E30FD085-D4BE-4DCF-B0B3-FF4B66C4389B',@T3_Mode_1_0=N''



IF OBJECT_ID('tempdb..#CognosDataGroupingFlags') IS NOT NULL
    DROP TABLE #CognosDataGroupingFlags

CREATE TABLE #CognosDataGroupingFlags
(
    T4_Code         nvarchar(15),
    T4_Signage      char(1),    
    T4_Company      char(1),
    T4_Currency     char(1),
    T4_Branch       tinyint,
    T4_Mode         tinyint,
    T4_BusinessType tinyint,
    T4_Geographical tinyint
)


IF OBJECT_ID('tempdb..#CognosExport') IS NOT NULL
    DROP TABLE #CognosExport

CREATE TABLE #CognosExport
(
    T5_AG                   uniqueidentifier,    
    T5_CompanyCode          nvarchar(8),
    T5_Mode                 nvarchar(4),
    T5_Branch               nvarchar(3),
    T5_BusinessType         nvarchar(3),
    T5_Amount               money,
    T5_TransactionCurrency  char(3),
    T5_TransactionAmount    money,
    T5_Geographical         nvarchar(3)
)